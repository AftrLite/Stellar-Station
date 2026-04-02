// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Projectiles;
using Content.Stellar.Shared.Weapons;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Stellar.Server.Weapons;

public sealed partial class StellarGunSystem : SharedStellarGunSystem
{
    // [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarGunTypesReloadableComponent, StellarProjectileEvent>(OnProjectile);
    }

    private void OnProjectile(Entity<StellarGunTypesReloadableComponent> ent, ref StellarProjectileEvent args)
    {
        var shootable = Spawn(ent.Comp.Shootable, TransformSystem.GetMapCoordinates(ent));
        var physics = EnsureComp<PhysicsComponent>(shootable);
        var projectile = EnsureComp<ProjectileComponent>(shootable);
        var stellarProjectile = EnsureComp<StellarGunProjectileComponent>(shootable);
        var targetMapVelocity = args.InitialSpeed + args.Direction.Normalized() * stellarProjectile.ProjectileSpeed;
        var currentMapVelocity = Physics.GetMapLinearVelocity(shootable, physics);
        var finalLinear = physics.LinearVelocity + targetMapVelocity - currentMapVelocity;
        projectile.Weapon = args.Gun;
        _projectile.SetShooter(shootable, projectile, args.User);
        Physics.SetLinearVelocity(shootable, finalLinear, body: physics);
        Physics.SetBodyStatus(shootable, physics, BodyStatus.InAir);
        TransformSystem.SetWorldRotation(shootable, args.Direction.ToWorldAngle() + projectile.Angle);
    }

    protected override void StellarHitscan(EntityUid gunUid, StellarHitscanEvent message, EntityUid? user = null)
    {
        var filter = Filter.Pvs(gunUid, entityManager: EntityManager);

        if (TryComp<ActorComponent>(user, out var actor))
            filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(message, filter);
    }

    protected override void StellarMuzzleFlash(EntityUid gunUid, StellarMuzzleFlashEvent message, EntityUid? user = null)
    {
        var filter = Filter.Pvs(gunUid, entityManager: EntityManager);

        if (TryComp<ActorComponent>(user, out var actor))
            filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(message, filter);
    }
}
