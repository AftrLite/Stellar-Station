// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Science;

public abstract class SharedStellarScienceAnalystSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSensorTerminalComponent, ActivateInWorldEvent>(OnTerminalActivateInWorld);
        SubscribeLocalEvent<StellarSensorTerminalComponent, InteractHandEvent>(OnTerminalInteractHand);

        SubscribeLocalEvent<StellarSensorTowerComponent, StellarSensorTowerRadialMessage>(OnTowerMenu);
        SubscribeLocalEvent<StellarSensorTowerComponent, ActivateInWorldEvent>(OnTowerActivateInWorld);
        SubscribeLocalEvent<StellarSensorTowerComponent, InteractHandEvent>(OnTowerInteractHand);
        SubscribeLocalEvent<StellarSensorTowerComponent, InteractUsingEvent>(OnTowerInteractUsing);

        SubscribeLocalEvent<StellarSensorTowerComponent, ExaminedEvent>(OnTowerExamined);
        SubscribeLocalEvent<StellarSensorTerminalComponent, ExaminedEvent>(OnTerminalExamined);
        SubscribeLocalEvent<StellarDataDriveComponent, ExaminedEvent>(OnDriveExamined);
    }

    #region Terminal
    private void OnTerminalActivateInWorld(Entity<StellarSensorTerminalComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.User == args.Target || ent.Comp.SyncingTower == null)
            return;

        args.Handled = HandleTerminal(ent, ent.Comp.SyncingTower.Value, args.User);
    }

    private void OnTerminalInteractHand(Entity<StellarSensorTerminalComponent> ent, ref InteractHandEvent args)
    {
        if (args.User == args.Target || args.Handled || ent.Comp.SyncingTower == null)
            return;

        args.Handled = HandleTerminal(ent, ent.Comp.SyncingTower.Value, args.User);
    }

    private void SyncTowerToTerminal(Entity<StellarSensorTerminalComponent> terminal, Entity<StellarSensorTowerComponent> tower)
    {
        terminal.Comp.SyncingTower = tower;
        terminal.Comp.TimeOutTimer = Timing.CurTime + terminal.Comp.TimeOut;
        terminal.Comp.State = StellarSensorTerminalState.Ringing;
        tower.Comp.State = StellarSensorTowerState.Ringing;
        _ambient.SetAmbience(terminal, true);
        _ambient.SetAmbience(tower, true);
        Popup.PopupPredicted(Loc.GetString("sensortower-popup-requestsync"), tower, tower, PopupType.Large);
        Popup.PopupPredicted(Loc.GetString("sensorterminal-popup-requestsync"), terminal, terminal, PopupType.Large);
        Dirty(terminal);
        Dirty(tower);
    }

    private bool HandleTerminal(Entity<StellarSensorTerminalComponent> terminal, Entity<StellarSensorTowerComponent?> tower, EntityUid user)
    {
        if (!Resolve(tower, ref tower.Comp))
            return false;

        switch (terminal.Comp.State)
        {
            case StellarSensorTerminalState.Ringing:
            {
                terminal.Comp.State = StellarSensorTerminalState.Active;
                tower.Comp.State = StellarSensorTowerState.Syncing;
                _ambient.SetAmbience(terminal, false);
                _ambient.SetAmbience(tower, false);
                Popup.PopupPredicted(Loc.GetString("sensortower-popup-syncstarted"), tower, tower, PopupType.Large);
                SetupTerminal(terminal);
                Dirty(tower);
                return true;
            }
            case StellarSensorTerminalState.Idle:
                return false; // Do stuff here.
        }
        return false;
    }

    private void SetupTerminal(Entity<StellarSensorTerminalComponent> ent)
    {
        var code = "";
        for (var i = 0; i < ent.Comp.CodeLength; i++)
        {
            var c = Random.Next(1, 6 + 1);
            code += $"{c}";
        }

        ent.Comp.TowerCode = code;
        ent.Comp.TimeOutTimer = Timing.CurTime + ent.Comp.TimeOut * 2;
        ent.Comp.PopupEffect = PredictedSpawnAttachedTo("StellarEffectSensorTerminalPopup", Transform(ent).Coordinates);
        Dirty(ent);
        RaiseNetworkEvent(new StellarSensorTerminalCodeEvent(GetNetEntity(ent)));
    }
    #endregion

    #region Towers

    /// <summary>
    /// Handles the radial menu input on the tower.
    /// </summary>
    private void OnTowerMenu(Entity<StellarSensorTowerComponent> ent, ref StellarSensorTowerRadialMessage args)
    {
        if (!TryComp<StellarSensorTerminalComponent>(ent.Comp.LinkedTerminal, out var terminalComp))
            return;

        var terminal = ent.Comp.LinkedTerminal;

        if (args.Method == TowerMenuMethod.Cancel)
        {
            Popup.PopupPredicted(Loc.GetString("sensortower-popup-synccancelled"), ent, args.Actor, PopupType.Large);
            Popup.PopupPredicted(Loc.GetString("sensorterminal-popup-synccancelled"), terminal.Value, terminal.Value, PopupType.LargeCaution);
            ResetConnection((ent.Owner, ent.Comp), (terminal.Value, terminalComp));
            return;
        }

        terminalComp.EnteredCode += args.CodeNumber.ToString(); // This little string is what updates the Tower's Sync Code.

        if (terminalComp.EnteredCode == terminalComp.TowerCode) // You got the sync code correct!
        {
            terminalComp.EnteredCode = string.Empty;
            terminalComp.TowerCode = string.Empty;
            terminalComp.State = StellarSensorTerminalState.Idle;
            ent.Comp.State = StellarSensorTowerState.Processing;
            _ui.CloseUi(ent.Owner, StellarSensorTowerRadialKey.Key);
            Popup.PopupPredicted(Loc.GetString("sensortower-popup-synced"), ent, args.Actor, PopupType.Large);
            Popup.PopupPredicted(Loc.GetString("sensorterminal-popup-synced"), terminal.Value, args.Actor, PopupType.Large);
            RaiseNetworkEvent(new StellarSensorTerminalCodeEvent(GetNetEntity(terminal.Value), true));

            var randTime = Random.Next(ent.Comp.ProcessingTimeMin, ent.Comp.ProcessingTimeMax);
            var doAfterArgs = new DoAfterArgs(EntityManager, ent, randTime, new StellarSensorTowerDoAfter(), ent, ent)
            {
                NeedHand = false,
                BreakOnWeightlessMove = false,
                BreakOnMove = false,
                BreakOnDamage = false,
                RequireCanInteract = false,
            };
            _doAfter.TryStartDoAfter(doAfterArgs);

            Dirty(ent);
            Dirty(terminal.Value, terminalComp);
            return;
        }

        if (terminalComp.EnteredCode.Length >= terminalComp.TowerCode.Length) // You messed up the sync code.
        {
            Popup.PopupPredicted(Loc.GetString("sensortower-popup-failed"), ent, args.Actor, PopupType.LargeCaution);
            Popup.PopupPredicted(Loc.GetString("sensorterminal-popup-failed"), terminal.Value, terminal.Value, PopupType.LargeCaution);
            ResetConnection((ent.Owner, ent.Comp), (terminal.Value, terminalComp));
        }
        Dirty(ent);
        Dirty(terminal.Value, terminalComp);
    }

    /// <summary>
    /// Reset the status on both a tower and the main terminal.
    /// </summary>
    protected void ResetConnection(Entity<StellarSensorTowerComponent?> tower, Entity<StellarSensorTerminalComponent?> terminal)
    {
        if (!Resolve(tower, ref tower.Comp) || !Resolve(terminal, ref terminal.Comp))
            return;

        terminal.Comp.EnteredCode = string.Empty;
        terminal.Comp.TowerCode = string.Empty;
        terminal.Comp.State = StellarSensorTerminalState.Idle;
        tower.Comp.State = StellarSensorTowerState.Idle;
        _ambient.SetAmbience(tower, false);
        _ambient.SetAmbience(terminal, false);
        _ui.CloseUi(tower.Owner, StellarSensorTowerRadialKey.Key);
        RaiseNetworkEvent(new StellarSensorTerminalCodeEvent(GetNetEntity(terminal), true));
        Dirty(tower);
        Dirty(terminal);
    }

    /// <summary>
    /// For serving radial menu.
    /// </summary> >
    private void OnTowerActivateInWorld(Entity<StellarSensorTowerComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.User == args.Target || ent.Comp.LinkedTerminal == null)
            return;

        args.Handled = CanUI(ent, ent.Comp.LinkedTerminal.Value, args.User);
    }

    /// <summary>
    /// For retrieving Full Data Drives from the tower, or serving the radial menu.
    /// </summary>
    private void OnTowerInteractHand(Entity<StellarSensorTowerComponent> ent, ref InteractHandEvent args)
    {
        if (args.User == args.Target || args.Handled || ent.Comp.LinkedTerminal == null)
            return;

        if (ent.Comp.State == StellarSensorTowerState.FullDrive)
        {
            ent.Comp.State = StellarSensorTowerState.NoDrive;
            var drive = PredictedSpawnNextToOrDrop(ent.Comp.FullDrive, ent);
            Audio.PlayPredicted(ent.Comp.EjectSound, ent, ent);
            _hands.TryPickupAnyHand(args.User, drive);
            Dirty(ent);
            args.Handled = true;
            return;
        }

        args.Handled = CanUI(ent, ent.Comp.LinkedTerminal.Value, args.User);
    }

    /// <summary>
    /// For inserting Empty Data Drives into the tower.
    /// </summary>
    private void OnTowerInteractUsing(Entity<StellarSensorTowerComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || ent.Comp.State != StellarSensorTowerState.NoDrive || !TryComp<StellarDataDriveComponent>(args.Used, out var drive))
            return;

        if (drive.Full)
        {
            Popup.PopupClient(Loc.GetString("sensortower-popup-drivefull"), Transform(ent).Coordinates, args.User);
            return;
        }

        PredictedQueueDel(args.Used);
        args.Handled = true;
        ent.Comp.State = StellarSensorTowerState.Idle;
        Audio.PlayPredicted(ent.Comp.InsertSound, ent, args.User);
        Dirty(ent);
    }

    /// <summary>
    /// Do we serve the radial menu to a user? Also, AftrLite got was working very fast and probably used more If-statements than he needed to.
    /// </summary>
    private bool CanUI(Entity<StellarSensorTowerComponent> tower, Entity<StellarSensorTerminalComponent?> terminal, EntityUid user)
    {
        if (!Resolve(terminal, ref terminal.Comp))
            return false;

        switch (tower.Comp.State)
        {
            case StellarSensorTowerState.NoDrive:
                Popup.PopupClient(Loc.GetString("sensortower-popup-nodrive"), tower, user);
                return false;
            case StellarSensorTowerState.Ringing:
                Popup.PopupClient(Loc.GetString("sensortower-popup-ringing"), tower, user);
                return false;
            case StellarSensorTowerState.Processing:
                Popup.PopupClient(Loc.GetString("sensortower-popup-processing"), tower, user);
                return false;
        }

        if (terminal.Comp.State == StellarSensorTerminalState.Active && tower.Comp.State != StellarSensorTowerState.Syncing || terminal.Comp.State == StellarSensorTerminalState.Ringing && tower.Comp.State != StellarSensorTowerState.Syncing)
        {
            Popup.PopupEntity(Loc.GetString("sensortower-popup-occupied"), tower, user);
            return false;
        }

        if (tower.Comp.State == StellarSensorTowerState.Idle)
        {
            SyncTowerToTerminal((terminal.Owner, terminal.Comp) ,tower);
            return true;
        }

        if (tower.Comp.State == StellarSensorTowerState.Syncing)
        {
            _ui.OpenUi(tower.Owner, StellarSensorTowerRadialKey.Key, user, true);
            return true;
        }
        return false;
    }
    #endregion

    #region Examine
    private void OnTowerExamined(Entity<StellarSensorTowerComponent> ent, ref ExaminedEvent args)
    {
        switch (ent.Comp.State)
        {
            case StellarSensorTowerState.NoDrive:
                args.PushMarkup(Loc.GetString("sensortower-examine-driveempty"));
                break;
            case StellarSensorTowerState.FullDrive:
                args.PushMarkup(Loc.GetString("sensortower-examine-drivefull"));
                break;
            case StellarSensorTowerState.Idle:
                args.PushMarkup(Loc.GetString("sensortower-examine-idle"));
                break;
            case StellarSensorTowerState.Processing:
                args.PushMarkup(Loc.GetString("sensortower-examine-processing"));
                break;
            case StellarSensorTowerState.Ringing:
                args.PushMarkup(Loc.GetString("sensortower-examine-ringing"));
                break;
            case StellarSensorTowerState.Syncing:
                args.PushMarkup(Loc.GetString("sensortower-examine-syncing"));
                break;
        }
    }

    private void OnTerminalExamined(Entity<StellarSensorTerminalComponent> ent, ref ExaminedEvent args)
    {
        switch (ent.Comp.State)
        {
            case StellarSensorTerminalState.Active:
                args.PushMarkup(Loc.GetString("sensorterminal-examine-active"));
                break;
            case StellarSensorTerminalState.Idle:
                args.PushMarkup(Loc.GetString("sensorterminal-examine-idle"));
                break;
            case StellarSensorTerminalState.Ringing:
                args.PushMarkup(Loc.GetString("sensorterminal-examine-ringing"));
                break;
        }
    }

    private void OnDriveExamined(Entity<StellarDataDriveComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.Full)
            args.PushMarkup(Loc.GetString("datadrive-examine-empty"));
        else
            args.PushMarkup(Loc.GetString("datadrive-examine-full"));
    }
    #endregion
}


[Serializable, NetSerializable]
public sealed partial class StellarSensorTowerDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class StellarSensorTowerRadialMessage(TowerMenuMethod method, int? codeNumber = null) : BoundUserInterfaceMessage
{
    public TowerMenuMethod Method = method;

    public int? CodeNumber = codeNumber;
}

[Serializable, NetSerializable]
public sealed class StellarSensorTerminalCodeEvent(NetEntity target, bool end = false) : EntityEventArgs
{
    public NetEntity Target = target;

    public bool End = end;
}

[Serializable, NetSerializable]
public enum TowerMenuMethod : byte
{
    CodeInput,
    Cancel,
}
