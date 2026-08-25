// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Linq;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._ES.Camera;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Stellar.Shared.CCVars;
using Content.Stellar.Shared.Overcharge.Components;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Stellar.Server.Science;

public sealed partial class StellarScienceBluespaceDriveSystem : SharedStellarScienceBluespaceDriveSystem
{
    [Dependency] private readonly DockingSystem _dock = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popUp = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;

    private TimeSpan _startupTime; // = TimeSpan.FromSeconds(10);
    private TimeSpan _chargeTimeMax; // = TimeSpan.FromSeconds(60);
    private TimeSpan _chargeTimeMin; // = TimeSpan.FromSeconds(61);
    private TimeSpan _leaveTime; // = TimeSpan.FromSeconds(71);

    private int _playerMin;
    private int _playerMax;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, STCCVars.StationWakeupTime,  value => _startupTime = TimeSpan.FromSeconds(value), true);
        Subs.CVar(_config, STCCVars.DriveChargeTimeMax, value => _chargeTimeMax = TimeSpan.FromMinutes(value), true);
        Subs.CVar(_config, STCCVars.DriveChargeTimeMin, value => _chargeTimeMin = TimeSpan.FromMinutes(value), true);
        Subs.CVar(_config, STCCVars.DriveAutomaticLeaveTime, value => _leaveTime = TimeSpan.FromMinutes(value), true);
        Subs.CVar(_config, STCCVars.MinExpectedPlayers,  value => _playerMin = value, true);
        Subs.CVar(_config, CCVars.SoftMaxPlayers, value => _playerMax = value, true);

        SubscribeLocalEvent<StellarBluespaceDriveCoreComponent, MapInitEvent>(OnCoreInit);
        SubscribeLocalEvent<StellarBluespaceDrivePartComponent, MapInitEvent>(OnPartInit);

        SubscribeLocalEvent<StellarDriveCrisisEvent>(OnDriveCrisis);
        SubscribeLocalEvent<StellarBluespaceDriveCoreComponent, StellarDriveChargeEvent>(OnDriveCharge);
        SubscribeLocalEvent<StellarBluespaceDriveCoreComponent, StellarDriveCountdownDoAfter>(OnCountdownDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var finaleQuery = EntityQueryEnumerator<StellarBluespaceDriveCoreComponent>();
        while (finaleQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.StartupTimer is { } startupTimer && _timing.CurTime >= startupTimer)
            {
                comp.StartupTimer = null;
                comp.DriveState = StellarBluespaceDriveState.Charging;
                comp.AutoLeaveTimer = _leaveTime + _timing.CurTime;
                comp.RechargeStartedAt = _timing.CurTime;

                _appearance.SetData(uid, StellarBluespaceDriveVisuals.Visuals, 1);
                UpdateRechargeTime((uid, comp));
                Dirty(uid, comp);
            }

            if (comp.AutoLeaveTimer is { } autoTimer && _timing.CurTime >= autoTimer && !comp.CrisisBlocked)
            {
                comp.AutoLeaveTimer = null;
                comp.DriveState =  StellarBluespaceDriveState.Spooling;
                var mins = comp.CountdownTime.Minutes;
                var secs = comp.CountdownTime.Seconds;

                var doArgs = new DoAfterArgs(EntityManager, uid, comp.CountdownTime, new StellarDriveCountdownDoAfter(), uid, uid)
                {
                    NeedHand = false,
                    BreakOnWeightlessMove = false,
                    BreakOnMove = false,
                    BreakOnDamage = false,
                    RequireCanInteract = false,
                };

                _chat.DispatchStationAnnouncement(uid, Loc.GetString("announcement-bsd-departure", ("minutesandseconds", $"{mins} minutes and {secs} seconds")), Loc.GetString("announcement-bsd-sender"));
                _appearance.SetData(uid, StellarBluespaceDriveVisuals.Visuals, StellarBluespaceDriveState.Active);
                _doAfter.TryStartDoAfter(doArgs, out var doAfterId);
                comp.DoAfterId = doAfterId;

                Dirty(uid, comp);
                continue;
            }

            if (comp.RechargeTimer is { } rechargeTimer && comp.DriveState == StellarBluespaceDriveState.Charging)
            {
                if (_timing.CurTime >= rechargeTimer)
                {
                    comp.Progress = 4;
                    comp.RechargeTimer = null;
                    comp.DriveState =  StellarBluespaceDriveState.Ready;
                    _appearance.SetData(uid, StellarBluespaceDriveVisuals.Visuals, StellarBluespaceDriveState.Ready);
                    _chat.DispatchStationAnnouncement(uid, Loc.GetString("announcement-bsd-charge", ("progress", comp.Progress)), Loc.GetString("announcement-bsd-sender"));
                    Dirty(uid, comp);
                    continue;
                }

                if ((_timing.CurTime - comp.RechargeStartedAt) / (rechargeTimer - comp.RechargeStartedAt) > 0.334 && comp.Progress < 2)
                {
                    comp.Progress = 2;
                    _appearance.SetData(uid, StellarBluespaceDriveVisuals.Visuals, comp.Progress);
                    _chat.DispatchStationAnnouncement(uid, Loc.GetString("announcement-bsd-charge", ("progress", comp.Progress)), Loc.GetString("announcement-bsd-sender"));
                    Dirty(uid, comp);
                }
                if ((_timing.CurTime - comp.RechargeStartedAt) / (rechargeTimer - comp.RechargeStartedAt) > 0.667 && comp.Progress < 3)
                {
                    comp.Progress = 3;
                    _appearance.SetData(uid, StellarBluespaceDriveVisuals.Visuals, comp.Progress);
                    _chat.DispatchStationAnnouncement(uid, Loc.GetString("announcement-bsd-charge", ("progress", comp.Progress)), Loc.GetString("announcement-bsd-sender"));
                    Dirty(uid, comp);
                }
            }
        }
    }

    private void OnDriveCrisis(ref StellarDriveCrisisEvent args)
    {
        var coreQuery = AllEntityQuery<StellarBluespaceDriveCoreComponent>();
        while (coreQuery.MoveNext(out var uid, out var comp))
        {
            if (args.Active && _doAfter.IsRunning(comp.DoAfterId))
                _doAfter.Cancel(comp.DoAfterId);

            if (!args.Active && !_doAfter.IsRunning(comp.DoAfterId))
            {
                var autoTimer = _leaveTime + comp.RechargeStartedAt;
                if (_timing.CurTime >= autoTimer)
                {
                    comp.AutoLeaveTimer = null;
                    var timeLeft = comp.ResumeTime != null ? comp.ResumeTime.Value : comp.CountdownTime;
                    var doArgs = new DoAfterArgs(EntityManager, uid, timeLeft, new StellarDriveCountdownDoAfter(), uid, uid)
                    {
                        NeedHand = false,
                        BreakOnWeightlessMove = false,
                        BreakOnMove = false,
                        BreakOnDamage = false,
                        RequireCanInteract = false,
                    };
                    _chat.DispatchStationAnnouncement(uid, Loc.GetString("announcement-bsd-departure-brief", ("minutesandseconds", $"{timeLeft.Minutes} minutes and {timeLeft.Seconds} seconds")), Loc.GetString("announcement-bsd-sender"));
                    _appearance.SetData(uid, StellarBluespaceDriveVisuals.Visuals, StellarBluespaceDriveState.Active);
                    _doAfter.TryStartDoAfter(doArgs, out var doAfterId);
                    comp.DoAfterId = doAfterId;
                }
            }

            comp.CrisisBlocked = args.Active;
            Dirty(uid, comp);
        }
    }

    private void OnDriveCharge(Entity<StellarBluespaceDriveCoreComponent> ent, ref StellarDriveChargeEvent args)
    {
        var mult = 1;
        if (TryComp<StellarOverchargeableComponent>(ent, out var overchargeable))
        {
            switch (overchargeable.State)
            {
                case OverchargeState.Disabled:
                    _popUp.PopupPredicted(Loc.GetString("bsd-popup-charge-gain-normal"), ent, ent, PopupType.Medium);
                    break;
                case OverchargeState.Overcharged:
                    _popUp.PopupPredicted(Loc.GetString("bsd-popup-charge-gain-overcharge"), ent, ent, PopupType.Large);
                    mult = 2;
                    break;
                case OverchargeState.Hypercharged:
                    _popUp.PopupPredicted(Loc.GetString("bsd-popup-charge-gain-hypercharge"), ent, ent, PopupType.Large);
                    mult = 4;
                    break;
            }
        }

        ent.Comp.DepositedDataDrives += args.Data * mult;
        ent.Comp.DepositedEnergy += args.Energy * mult;
        UpdateRechargeTime(ent);
    }

    private void OnCountdownDoAfter(Entity<StellarBluespaceDriveCoreComponent> ent, ref StellarDriveCountdownDoAfter args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            var timeLeft = ent.Comp.ResumeTime != null ? ent.Comp.ResumeTime.Value : ent.Comp.CountdownTime;
            ent.Comp.ResumeTime = timeLeft - (args.DoAfter.CancelledTime - args.DoAfter.StartTime);
            return;
        }

        ent.Comp.DriveState =  StellarBluespaceDriveState.Active;
        _appearance.SetData(ent, StellarBluespaceDriveVisuals.Visuals, StellarBluespaceDriveState.Active);
        if (Transform(ent).GridUid is { } gridUid && TryComp<ShuttleComponent>(gridUid, out var shuttleComp))
        {
            var translation = new ESScreenshakeParameters() { Trauma = 0.65f, DecayRate = 0.1f, Frequency = 0.008f };
            var filter = Filter.BroadcastGrid(gridUid);

            _shake.Screenshake(filter, translation, null);
            _dock.SetDockBolts(gridUid, true);
            _shuttle.Enable(gridUid);
            _shuttle.FTLToCoordinates(gridUid, shuttleComp, Transform(ent).Coordinates, Angle.Zero, 0f, 9999f); // This FTL is never meant to complete. Thus, 9999 time.
            _roundEnd.EndRound();
            args.Handled = true;
        }

        Dirty(ent);
    }

    private void UpdateRechargeTime(Entity<StellarBluespaceDriveCoreComponent> ent)
    {
        ent.Comp.TotalCrew = _playerMan.Sessions.Count(session => session.Status == SessionStatus.InGame && HasComp<HumanoidProfileComponent>(session.AttachedEntity));

#if DEBUG
        if (ent.Comp.TotalCrew < _playerMin)
            ent.Comp.TotalCrew = _playerMin;
#endif

        var maxTime = _chargeTimeMax;
        var minTime = _chargeTimeMin;

        var popScalar = 1.5 - Math.Clamp((float) (ent.Comp.TotalCrew - _playerMin) / (_playerMax - _playerMin), 0f, 1f) * 0.5f; // Used to scale the "boost" for pop. Less pop = bluespace drive charges faster.
        var energy = Math.Clamp((float) ent.Comp.DepositedEnergy / 50, 0f, 1f);
        var data = Math.Clamp((float) ent.Comp.DepositedDataDrives / 20, 0f, 1f);
        var lerp = Math.Clamp(((energy * 0.65) * popScalar) + ((data * 0.95) * popScalar), 0f, 1f);
        var lerpTime = MathHelper.Lerp(maxTime, minTime, lerp);

        ent.Comp.RechargeTimer = ent.Comp.RechargeStartedAt + lerpTime;
        Dirty(ent);
    }
}
