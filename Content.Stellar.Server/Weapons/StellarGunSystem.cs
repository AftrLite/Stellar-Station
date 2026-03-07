// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Stellar.Shared.Weapons;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Stellar.Server.Weapons;

public sealed partial class StellarGunSystem : SharedStellarGunSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarGunHitscanComponent, StellarGunShotEvent>(OnGunShot);
    }

    private void OnGunShot(Entity<StellarGunHitscanComponent> ent, ref StellarGunShotEvent args)
    {
        if (args.Gun is null || args.To is null)
            return;

        var fromMap = _transform.ToMapCoordinates(args.From).Position;
        var toMap = _transform.ToMapCoordinates(args.To.Value).Position;
        var mapDirection = toMap - fromMap;
        var mapAngle = mapDirection.ToAngle();
        var angle = GetRecoilAngle(_timing.CurTime, args.Gun, mapDirection.ToAngle());
        toMap = fromMap + angle.ToVec() * mapDirection.Length();
        mapDirection = toMap - fromMap;

        if (TryComp<StellarGunTypesReloadableComponent>(args.GunUid, out var gun) && gun.MultiShotAmount > 1)
        {
            var angles = LinearSpread(mapAngle - gun.MultiShotSpread / 2, mapAngle + gun.MultiShotSpread / 2, gun.MultiShotAmount);

            for (var i = 0; i < gun.MultiShotAmount; i++)
            {
                var hitscanEv = new HitscanTraceEvent()
                {
                    FromCoordinates = GetCoordinates(args.From),
                    ShotDirection = angles[i].ToVec(),
                    Gun = ent,
                    Shooter = args.UserUid,
                    Target = args.Gun.Target,
                };
                RaiseLocalEvent(ent, ref hitscanEv);
            }
        }
        else
        {
            var hitscanEv = new HitscanTraceEvent
            {
                FromCoordinates = GetCoordinates(args.From),
                ShotDirection = mapDirection.Normalized(),
                Gun = ent,
                Shooter = args.UserUid,
                Target = args.Gun.Target,
            };
            RaiseLocalEvent(ent, ref hitscanEv);
        }
    }

    private Angle GetRecoilAngle(TimeSpan curTime, GunComponent comp, Angle direction)
    {
        var timeSinceLastFire = (curTime - comp.LastFire).TotalSeconds;
        var newTheta = MathHelper.Clamp(comp.CurrentAngle.Theta + comp.AngleIncreaseModified.Theta - comp.AngleDecayModified.Theta * timeSinceLastFire, comp.MinAngleModified.Theta, comp.MaxAngleModified.Theta);
        comp.CurrentAngle = new Angle(newTheta);
        comp.LastFire = comp.NextFire;

        // Convert it so angle can go either side.
        var random = _random.NextFloat(-0.5f, 0.5f);
        var spread = comp.CurrentAngle.Theta * random;
        var angle = new Angle(direction.Theta + comp.CurrentAngle.Theta * random);
        DebugTools.Assert(spread <= comp.MaxAngleModified.Theta);
        return angle;
    }

    private Angle[] LinearSpread(Angle start, Angle end, int intervals)
    {
        var angles = new Angle[intervals];
        DebugTools.Assert(intervals > 1);

        for (var i = 0; i <= intervals - 1; i++)
        {
            angles[i] = new Angle(start + (end - start) * i / (intervals - 1));
        }

        return angles;
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
