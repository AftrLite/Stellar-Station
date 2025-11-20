// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Movement;

public sealed class StellarSprintSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly SharedResourceBarsSystem _resourceBars = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSprintComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StellarSprintComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<StellarSprintComponent, MoveInputEvent>(OnMoveInput);
        SubscribeLocalEvent<StellarSprintComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<StellarSprintComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnMapInit(Entity<StellarSprintComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Energy = ent.Comp.EnergyMax;
        _resourceBars.ShowResourceBar(ent.Owner, ent.Comp.ResourceBar, ent.Comp.Energy / ent.Comp.EnergyMax);
        _movementSpeedModifier.RefreshMovementSpeedModifiers(ent);
    }

    private void OnShutdown(Entity<StellarSprintComponent> ent, ref ComponentShutdown args)
    {
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBar);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StellarSprintComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.LastUpdate + comp.UpdateInterval < _timing.CurTime)
                continue;

            var last = comp.LastUpdate;
            comp.LastUpdate = _timing.CurTime;
            Dirty(uid, comp);

            if (comp.Sprinting)
                continue;

            if (comp.RegenerationCooldown >= _timing.CurTime)
                continue;

            var regen = (float)(_timing.CurTime - last).TotalSeconds * comp.RegenerationPerSecond * comp.RegenerationModifier;
            comp.Energy = Math.Min(comp.Energy + regen, comp.EnergyMax);

            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            _resourceBars.ShowResourceBar(uid, comp.ResourceBar, comp.Energy / comp.EnergyMax);
        }
    }

    private void OnMove(Entity<StellarSprintComponent> ent, ref MoveEvent args)
    {
        if (!ent.Comp.Sprinting)
            return;

        var distance = (args.NewPosition.Position - args.OldPosition.Position).Length();
        var decay = ent.Comp.DecayPerUnitMoved * distance * ent.Comp.DecayModifier;

        ent.Comp.Energy = Math.Max(ent.Comp.Energy - decay, 0f);
        Dirty(ent);

        _movementSpeedModifier.RefreshMovementSpeedModifiers(ent);
        _resourceBars.ShowResourceBar(ent.Owner, ent.Comp.ResourceBar, ent.Comp.Energy / ent.Comp.EnergyMax);
    }

    private void StartSprinting(Entity<StellarSprintComponent> ent)
    {
        ent.Comp.Sprinting = true;
        Dirty(ent);
    }

    private void StopSprinting(Entity<StellarSprintComponent> ent)
    {
        ent.Comp.RegenerationCooldown = _timing.CurTime + ent.Comp.RegenerationInterval;
        ent.Comp.Sprinting = false;
        Dirty(ent);
    }

    private void OnMoveInput(Entity<StellarSprintComponent> ent, ref MoveInputEvent args)
    {
        var isMoving = (args.Entity.Comp.HeldMoveButtons & MoveButtons.AnyDirection) != 0x0;
        var sprintButton = (args.Entity.Comp.HeldMoveButtons & MoveButtons.Walk) != 0x0;
        var trySprint = isMoving && sprintButton;

        if (trySprint && !ent.Comp.Sprinting && ent.Comp.Energy > ent.Comp.MinimumSprintEnergy)
            StartSprinting(ent);
        else if (!trySprint && ent.Comp.Sprinting)
            StopSprinting(ent);
    }

    private void OnRefresh(Entity<StellarSprintComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var modifier = Math.Clamp(1.5f * (ent.Comp.Energy / ent.Comp.EnergyMax) + 0.5f, 0.5f, 1f);

        args.ModifySpeed(1f, modifier);
    }
}
