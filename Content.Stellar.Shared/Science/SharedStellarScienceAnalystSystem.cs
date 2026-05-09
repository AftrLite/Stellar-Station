// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Science;

public abstract class SharedStellarScienceAnalystSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    // [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    protected Entity<StellarSensorTerminalComponent>? SensorTerminal;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSensorTerminalComponent, ComponentInit>(OnTerminalInit);
        SubscribeLocalEvent<StellarSensorTerminalComponent, ActivateInWorldEvent>(OnTerminalActivateInWorld);
        SubscribeLocalEvent<StellarSensorTerminalComponent, InteractHandEvent>(OnTerminalInteractHand);

        SubscribeLocalEvent<StellarSensorTowerComponent, StellarSensorTowerRadialMessage>(OnTowerMenu);
        SubscribeLocalEvent<StellarSensorTowerComponent, ActivateInWorldEvent>(OnTowerActivateInWorld);
        SubscribeLocalEvent<StellarSensorTowerComponent, InteractHandEvent>(OnTowerInteractHand);
    }

    #region Terminal
    private void OnTerminalInit(Entity<StellarSensorTerminalComponent> ent, ref ComponentInit args)
    {
        SensorTerminal = ent;
    }

    private void OnTerminalActivateInWorld(Entity<StellarSensorTerminalComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.User == args.Target)
            return;

        args.Handled = HandleTerminal(ent, args.User);
    }

    private void OnTerminalInteractHand(Entity<StellarSensorTerminalComponent> ent, ref InteractHandEvent args)
    {
        if (args.User == args.Target || args.Handled)
            return;

        args.Handled = HandleTerminal(ent, args.User);
    }

    private void SyncTowerToTerminal(Entity<StellarSensorTerminalComponent> terminal, Entity<StellarSensorTowerComponent> tower)
    {
        terminal.Comp.ConnectedTower = tower;
        terminal.Comp.TimeOutTimer = Timing.CurTime + terminal.Comp.TimeOut;
        terminal.Comp.State = StellarSensorTerminalState.Ringing;
        tower.Comp.State = StellarSensorTowerState.Ringing;
        _ambient.SetAmbience(terminal, true);
        _ambient.SetAmbience(tower, true);
        Popup.PopupPredicted("You request a sync code from the sensor network!", tower, tower, PopupType.Large); // TODO: Localization
        Popup.PopupPredicted("A sensor tower is requesting synchronization!", terminal, terminal, PopupType.Large); // TODO: Localization
        Dirty(terminal);
        Dirty(tower);
    }

    private bool HandleTerminal(Entity<StellarSensorTerminalComponent> ent, EntityUid user)
    {
        if (ent.Comp.ConnectedTower is not { } tower)
            return false;

        switch (ent.Comp.State)
        {
            case StellarSensorTerminalState.Ringing:
            {
                ent.Comp.State = StellarSensorTerminalState.Active;
                tower.Comp.State = StellarSensorTowerState.Syncing;
                _ambient.SetAmbience(ent, false);
                _ambient.SetAmbience(tower, false);
                if (_net.IsServer)
                    SetupTerminal(ent);
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
            var c = _random.Next(1, 6 + 1);
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

    private void OnTowerMenu(Entity<StellarSensorTowerComponent> ent, ref StellarSensorTowerRadialMessage args)
    {
        if (SensorTerminal is not { } terminal)
            return;

        if (args.Method == TowerMenuMethod.Cancel)
        {
            Popup.PopupPredicted("Synchronization cancelled!", ent, args.Actor, PopupType.Large); // TODO: Localization
            Popup.PopupPredicted("Sync aborted by connected tower!", terminal, terminal, PopupType.LargeCaution); // TODO: Localization
            ResetConnection(ent, terminal);
            return;
        }

        terminal.Comp.EnteredCode += args.CodeNumber.ToString();
        if (terminal.Comp.EnteredCode == terminal.Comp.TowerCode)
        {
            terminal.Comp.EnteredCode = string.Empty;
            terminal.Comp.TowerCode = string.Empty;
            terminal.Comp.State = StellarSensorTerminalState.Idle;
            ent.Comp.State = StellarSensorTowerState.Processing;
            _ui.CloseUi(ent.Owner, StellarSensorTowerRadialKey.Key);
            Popup.PopupPredicted("Synchronized!", ent, args.Actor, PopupType.Large); // TODO: Localization
            Popup.PopupPredicted("Sensor tower synced!", terminal, terminal, PopupType.Large); // TODO: Localization
            RaiseNetworkEvent(new StellarSensorTerminalCodeEvent(GetNetEntity(terminal), true));

            var randTime = _random.Next(ent.Comp.ProcessingTimeMin, ent.Comp.ProcessingTimeMax);
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
            Dirty(terminal);
            return;
        }

        if (terminal.Comp.EnteredCode.Length >= terminal.Comp.TowerCode.Length)
        {
            Popup.PopupPredicted("Synchronization failed!", ent, args.Actor, PopupType.LargeCaution); // TODO: Localization
            Popup.PopupPredicted("Sensor tower failed to sync!", terminal, terminal, PopupType.LargeCaution); // TODO: Localization
            ResetConnection(ent, terminal);
        }
        Dirty(ent);
        Dirty(terminal);
    }

    protected void ResetConnection(Entity<StellarSensorTowerComponent> tower, Entity<StellarSensorTerminalComponent> terminal)
    {
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

    private void OnTowerActivateInWorld(Entity<StellarSensorTowerComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.User == args.Target)
            return;

        args.Handled = CanUI(ent, args.User);
    }

    private void OnTowerInteractHand(Entity<StellarSensorTowerComponent> ent, ref InteractHandEvent args)
    {
        if (args.User == args.Target || args.Handled)
            return;

        args.Handled = CanUI(ent, args.User);
    }

    private bool CanUI(Entity<StellarSensorTowerComponent> ent, EntityUid user)
    {
        if (SensorTerminal is not { } terminal)
            return false;

        if (ent.Comp.State == StellarSensorTowerState.Ringing || ent.Comp.State == StellarSensorTowerState.NoDrive || ent.Comp.State == StellarSensorTowerState.Processing)
            return false;

        if (terminal.Comp.State == StellarSensorTerminalState.Active && ent.Comp.State != StellarSensorTowerState.Syncing || terminal.Comp.State == StellarSensorTerminalState.Ringing && ent.Comp.State != StellarSensorTowerState.Syncing)
        {
            Popup.PopupClient("The main terminal is occupied.", Transform(ent).Coordinates, user); // TODO: Localization
            return false;
        }

        if (ent.Comp.State == StellarSensorTowerState.Idle)
        {
            SyncTowerToTerminal(terminal, ent);
            return true;
        }

        if (ent.Comp.State == StellarSensorTowerState.Syncing)
        {
            _ui.OpenUi(ent.Owner, StellarSensorTowerRadialKey.Key, user, true);
            return true;
        }
        return false;
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
