// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Turrets;
using Content.Shared.Weapons.Ranged.Components;
using Content.Stellar.Shared.Overcharge.Components;

namespace Content.Stellar.Shared.Overcharge;

public sealed class SharedStellarOverchargeOffensiveSystem : EntitySystem
{
    [Dependency] private readonly SharedDeployableTurretSystem _turret = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarOverchargeableTurretComponent, StellarToggleOverchargeEvent>(OnOvercharged);
    }

    private void OnOvercharged(Entity<StellarOverchargeableTurretComponent> ent, ref StellarToggleOverchargeEvent args)
    {
        if (!TryComp<BatteryAmmoProviderComponent>(ent, out var ammo) || !TryComp<DeployableTurretComponent>(ent, out var turret))
            return;

        if (ent.Comp.OverchargeStates.TryGetValue(args.State, out var bullet))
        {
            _turret.TrySetState((ent, turret), true);
            ammo.Prototype = bullet;
            Dirty(ent, ammo);
        }
        else
        {
            _turret.TrySetState((ent, turret), false);
        }
    }
}
