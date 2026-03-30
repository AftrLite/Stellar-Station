// SPDX-FileCopyrightText: 2026 AftrLite
// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Stellar.Shared._ES.Core.Timer;

namespace Content.Stellar.Shared.Stellarator;

public sealed class StellarEngineInputSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ESEntityTimerSystem _esTimer = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarEngineInputComponent, StellarEngineRelayedEvent<StellarEngineShutdownEvent>>(OnShutdown);
        SubscribeLocalEvent<StellarEngineInputComponent, StellarEngineRelayedEvent<StellarEngineEjectFuelEvent>>(OnEjectFuel);
    }

    private void OnEjectFuel(Entity<StellarEngineInputComponent> ent, ref StellarEngineRelayedEvent<StellarEngineEjectFuelEvent> args)
    {
        _appearance.SetData(ent, EngineInputVisuals.EngineInput, EngineInputDoorState.Opening);
        _esTimer.SpawnMethodTimer(TimeSpan.FromSeconds(0.5), () => // Time based on the sprite anim speed
        {
            _appearance.SetData(ent, EngineInputVisuals.EngineInput, EngineInputDoorState.Open);
            var depletedRod = Spawn(ent.Comp.DepletedFuel, Transform(ent).Coordinates);
            _throwing.TryThrow(depletedRod, Transform(ent).Coordinates, 50f);
        });
        Dirty(ent);

        _popup.PopupEntity(Loc.GetString("popup-stellarator-refueling-available"), ent, PopupType.Large);
        // TODO: vfx + PVS-sfx.
    }

    private void OnShutdown(Entity<StellarEngineInputComponent> ent, ref StellarEngineRelayedEvent<StellarEngineShutdownEvent> args)
    {
        _appearance.SetData(ent, EngineInputVisuals.EngineInput, EngineInputDoorState.Open);
        _appearance.SetData(ent, EngineInputVisuals.EngineDisplay, EngineInputDisplayState.Off);

        if (args.Args.HasFuel)
        {
            var depletedRod = Spawn(ent.Comp.DepletedFuel, Transform(ent).Coordinates);
            _throwing.TryThrow(depletedRod, Transform(ent).Coordinates, 50f);
        }
    }
}
