// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.Effects;
using Content.Shared.Physics;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Weapons;

public abstract partial class SharedStellarGunSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarGunHitscanComponent, HitscanTraceEvent>(OnHitscan);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, StellarGunShotEvent>(OnGunShot);
        SubscribeLocalEvent<StellarGunHitscanComponent, HitscanDamageDealtEvent>(OnHitscanDamageDealt);
    }

    private void OnGunShot(Entity<StellarGunTypesReloadableComponent> ent, ref StellarGunShotEvent args)
    {
        if (args.Gun is null || args.To is null || args.Ammo.Uid is null)
            return;

        if (!TryComp<StellarGunHitscanComponent>(args.Ammo.Uid, out var hitscan))
            return;

        if (_netManager.IsClient && !_timing.IsFirstTimePredicted) // Don't overpredict clients!
            return;

        _audio.PlayPredicted(args.Gun.SoundGunshotModified, args.GunUid, args.UserUid);

        var ammo = args.Ammo.Uid;
        var fromMap = _transform.ToMapCoordinates(args.From).Position;
        var toMap = _transform.ToMapCoordinates(args.To.Value).Position;
        var mapDirection = toMap - fromMap;
        var angle = GetRecoilAngle(_timing.CurTime, args.GunUid, args.Gun, mapDirection.ToAngle());
        toMap = fromMap + angle.ToVec() * mapDirection.Length();
        mapDirection = toMap - fromMap;

        var ev = new StellarMuzzleFlashEvent(GetNetEntity(ent), hitscan.MuzzleFlash, mapDirection.ToAngle());
        StellarMuzzleFlash(args.GunUid, ev, ent);

        if (hitscan.MultiShotAmount > 1)
        {
            var angles = LinearSpread(mapDirection.ToAngle() - hitscan.MultiShotSpread / 2, mapDirection.ToAngle() + hitscan.MultiShotSpread / 2, hitscan.MultiShotAmount);

            for (var i = 0; i < hitscan.MultiShotAmount; i++)
            {
                var angleWiggle = _random.NextAngle(hitscan.MultiShotWiggleMin, hitscan.MultiShotWiggleMax) + angles[i];
                var hitscanEv = new HitscanTraceEvent
                {
                    FromCoordinates = GetCoordinates(args.From),
                    ShotDirection = angleWiggle.ToVec(),
                    Gun = ent,
                    Shooter = args.UserUid,
                    Target = args.Gun.Target,
                };
                RaiseLocalEvent(ammo.Value, ref hitscanEv);
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
            RaiseLocalEvent(ammo.Value, ref hitscanEv);
        }
    }

    private void OnHitscan(Entity<StellarGunHitscanComponent> ent, ref HitscanTraceEvent args)
    {
        var bullet = new SpriteSpecifier.Rsi(ent.Comp.Ray.RsiPath, "bullet");
        var start = new SpriteSpecifier.Rsi(ent.Comp.Ray.RsiPath, "start");
        var middle = new SpriteSpecifier.Rsi(ent.Comp.Ray.RsiPath, "middle");
        var end = new SpriteSpecifier.Rsi(ent.Comp.Ray.RsiPath, "end");

        var ev = new StellarHitscanEvent(
            ent.Comp.CollisionMask,
            GetNetCoordinates(args.FromCoordinates),
            args.ShotDirection,
            GetNetEntity(args.Gun),
            GetNetEntity(args.Shooter),
            GetNetEntity(args.Target),
            ent.Comp.Unshaded,
            ent.Comp.LightColor,
            ent.Comp.MaxDistance,
            ent.Comp.MuzzleFlash,
            bullet,
            start,
            middle,
            end);
        StellarHitscan(args.Gun, ev, args.Shooter);
    }

    private void OnHitscanDamageDealt(Entity<StellarGunHitscanComponent> ent, ref HitscanDamageDealtEvent args)
    {
        if (Deleted(args.Target))
            return;

        if (ent.Comp.HitColor != null && args.DamageDealt.GetTotal() != 0)
        {
            _color.RaiseEffect(ent.Comp.HitColor.Value,
                new List<EntityUid> { args.Target },
                Filter.Pvs(args.Target, entityManager: EntityManager));
        }
    }

    private Angle GetRecoilAngle(TimeSpan curTime, EntityUid uid, GunComponent comp, Angle direction)
    {
        var timeSinceLastFire = (curTime - comp.LastFire).TotalSeconds;
        var newTheta = MathHelper.Clamp(comp.CurrentAngle.Theta + comp.AngleIncreaseModified.Theta - comp.AngleDecayModified.Theta * timeSinceLastFire, comp.MinAngleModified.Theta, comp.MaxAngleModified.Theta);
        comp.CurrentAngle = new Angle(newTheta);
        comp.LastFire = comp.NextFire;
        DirtyFields(uid, comp, null, nameof(GunComponent.CurrentAngle), nameof(GunComponent.LastFire));

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

    protected abstract void StellarHitscan(EntityUid gunUid, StellarHitscanEvent message, EntityUid? user = null);

    protected abstract void StellarMuzzleFlash(EntityUid gunUid, StellarMuzzleFlashEvent message, EntityUid? user = null);
}

public enum StellarHitscanLayers : byte
{
    Unshaded,
    Shaded,
}

[Serializable, NetSerializable]
public sealed class StellarMuzzleFlashEvent : EntityEventArgs
{
    public NetEntity Uid;
    public string Prototype;
    public Angle Angle;

    public StellarMuzzleFlashEvent(NetEntity uid, string prototype, Angle angle)
    {
        Uid = uid;
        Prototype = prototype;
        Angle = angle;
    }
}

[Serializable, NetSerializable]
public sealed class StellarHitscanEvent : EntityEventArgs
{
    public CollisionGroup CollisionMask;

    public NetCoordinates FromCoordinates;

    public Vector2 ShotDirection;

    public NetEntity Gun;

    public NetEntity? Shooter;

    public NetEntity? Target;

    public bool Unshaded;

    public Color LightColor;

    public float MaxDistance;

    public SpriteSpecifier.Rsi RayStart;

    public SpriteSpecifier.Rsi RayMiddle;

    public SpriteSpecifier.Rsi RayEnd;

    public SpriteSpecifier.Rsi RayBullet;

    public EntProtoId MuzzleFlash;

    public StellarHitscanEvent(CollisionGroup collisionMask, NetCoordinates fromCoords, Vector2 shotDirection, NetEntity gun, NetEntity? shooter, NetEntity? target, bool unshaded, Color lightColor, float maxDist, EntProtoId muzzleFlash, SpriteSpecifier.Rsi rayBullet, SpriteSpecifier.Rsi rayStart, SpriteSpecifier.Rsi rayMiddle, SpriteSpecifier.Rsi rayEnd )
    {
        CollisionMask = collisionMask;
        FromCoordinates = fromCoords;
        ShotDirection = shotDirection;
        Gun = gun;
        Shooter = shooter;
        Target = target;
        Unshaded = unshaded;
        LightColor = lightColor;
        MaxDistance = maxDist;
        MuzzleFlash = muzzleFlash;
        RayBullet = rayBullet;
        RayStart = rayStart;
        RayMiddle = rayMiddle;
        RayEnd = rayEnd;
    }
}
