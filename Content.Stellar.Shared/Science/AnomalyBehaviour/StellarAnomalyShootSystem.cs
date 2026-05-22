// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening


using System.Numerics;
using Content.Stellar.Shared.Science.Components;
using Content.Stellar.Shared.Weapons;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Stellar.Shared.Science.AnomalyBehaviour;

public sealed class StellarAnomalyShootSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStellarGunSystem _gun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyPulseShootComponent, StellarAnomalyPulseEvent>(OnPulse);
    }

    private void OnPulse(Entity<StellarAnomalyPulseShootComponent> ent, ref StellarAnomalyPulseEvent args)
    {
        var xform = Transform(ent);
        var toMap = _transform.ToMapCoordinates(new EntityCoordinates(ent, new Vector2(0, -1))).Position;
        var fromMap = _transform.GetMapCoordinates(ent).Position;
        var mapDirection = toMap - fromMap;
        var shootAmount = _random.Next(ent.Comp.ShootAmountMin, ent.Comp.ShootAmountMax + 1);

        for (int i = 0; i < shootAmount; i++)
        {
            var direction = mapDirection * _random.NextAngle().ToVec().Normalized();

            if (ent.Comp.ShootingMethod == StellarGunMethod.Hitscan)
                _gun.ConstructHitscan(ent, ent.Comp.Shootable, xform.Coordinates, direction);
            else
                _gun.ConstructProjectile(ent, ent.Comp.Shootable,  direction, null, ent.Comp.ProjectileSpeed);
        }
    }
}
