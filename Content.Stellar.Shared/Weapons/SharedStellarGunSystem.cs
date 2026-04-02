// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared._ES.Camera;
using Content.Shared.Effects;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Weapons;

public abstract partial class SharedStellarGunSystem : EntitySystem
{
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeTypes();

        SubscribeLocalEvent<StellarGunHitscanComponent, HitscanTraceEvent>(OnHitscan);
        SubscribeLocalEvent<StellarGunHitscanComponent, HitscanDamageDealtEvent>(OnHitscanDamageDealt);

        SubscribeLocalEvent<StellarGunTypesReloadableComponent, TakeAmmoEvent>(OnAmmoUsed);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, AttemptShootEvent>(OnAttemptShoot);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, StellarGunShotEvent>(OnGunShot);
    }

    private void OnAmmoUsed(Entity<StellarGunTypesReloadableComponent> ent, ref TakeAmmoEvent args)
    {
        if (ent.Comp.AmmoCount <= 0)
            return;

        if (ent.Comp.AmmoCount != null)
            ent.Comp.AmmoCount--;
        Dirty(ent);
        UpdateReloadableAppearance(ent);
    }

    private void OnAttemptShoot(Entity<StellarGunTypesReloadableComponent> ent, ref AttemptShootEvent args)
    {
        if (ent.Comp.AmmoCount <= 0)
        {
            if (TryComp<GunComponent>(ent, out var gunComp))
            {
                gunComp.NextFire = TimeSpan.FromSeconds(Math.Max(gunComp.NextFire.TotalSeconds + 0.5f, gunComp.NextFire.TotalSeconds));
                Dirty(ent, gunComp);
            }
            _popUp.PopupCursor(Loc.GetString("gun-magazine-fired-empty"));
            _audio.PlayPredicted(ent.Comp.SoundEmpty, ent, args.User);
            args.Cancelled = true;
        }
    }

    private void OnGunShot(Entity<StellarGunTypesReloadableComponent> ent, ref StellarGunShotEvent args)
    {
        if (args.Gun is null || args.To is null)
            return;

        var fromMap = TransformSystem.ToMapCoordinates(args.From).Position;
        var toMap = TransformSystem.ToMapCoordinates(args.To.Value).Position;
        var mapDirection = toMap - fromMap;
        var gunShakeTranslate = new ESScreenshakeParameters() { Trauma = 0.9f * args.Gun.CameraRecoilScalarModified, DecayRate = 15f, Frequency = 0.008f, Direction = mapDirection.Normalized()};
        var gunShakeRotate = new ESScreenshakeParameters() { Trauma = 0.075f * args.Gun.CameraRecoilScalarModified, DecayRate = 25f, Frequency = 0.012f};
        _shake.Screenshake(args.UserUid, gunShakeTranslate, gunShakeRotate);
        _audio.PlayPredicted(args.Gun.SoundGunshotModified, args.GunUid, args.UserUid);

        var angle = GetRecoilAngle(_timing.CurTime, args.GunUid, args.Gun, mapDirection.ToAngle());
        toMap = fromMap + angle.ToVec() * mapDirection.Length();
        mapDirection = toMap - fromMap;

        // Ramping firerate for guns that do that!
        if (ent.Comp.RampingFireRate is not null)
        {
            var lerp = Math.Clamp(args.Gun.ShotCounter / ent.Comp.RampingBulletsNeeded, 0, 1);
            args.Gun.FireRateModified = MathHelper.Lerp(args.Gun.FireRate, ent.Comp.RampingFireRate.Value, lerp);
            Dirty(args.GunUid, args.Gun);
            Log.Info($"firerate lerp is at {lerp}");
        }

        if (ent.Comp.ShootingMethod == StellarGunMethod.Hitscan && _timing.IsFirstTimePredicted)
            ShootHitscan(ent, args.From, mapDirection, args.UserUid, args.Gun.Target, Spawn(ent.Comp.Shootable));

        if (ent.Comp.ShootingMethod == StellarGunMethod.Projectile && _timing.IsFirstTimePredicted)
            ShootProjectile(ent, mapDirection, args.UserUid);

        if (ent.Comp.MuzzleFlash != null && _timing.IsFirstTimePredicted)
        {
            var ev = new StellarMuzzleFlashEvent(GetNetEntity(ent), ent.Comp.MuzzleFlash, mapDirection.ToAngle());
            StellarMuzzleFlash(args.GunUid, ev, ent);
        }
    }

    private void ShootHitscan(Entity<StellarGunTypesReloadableComponent> ent, NetCoordinates from, Vector2 direction, EntityUid user, EntityUid? target, EntityUid ammo)
    {
        if (ent.Comp.MultiShotAmount > 1)
        {
            var angles = LinearSpread(direction.ToAngle() - ent.Comp.MultiShotSpread / 2, direction.ToAngle() + ent.Comp.MultiShotSpread / 2, ent.Comp.MultiShotAmount);

            for (var i = 0; i < ent.Comp.MultiShotAmount; i++)
            {
                var angleWiggle = _random.NextAngle(ent.Comp.MultiShotWiggleMin, ent.Comp.MultiShotWiggleMax) + angles[i];
                var hitscanEv = new HitscanTraceEvent
                {
                    FromCoordinates = GetCoordinates(from), ShotDirection = angleWiggle.ToVec(), Gun = ent, Shooter = user, Target = target,
                };
                RaiseLocalEvent(ammo, ref hitscanEv);
            }
        }
        else
        {
            var hitscanEv = new HitscanTraceEvent
            {
                FromCoordinates = GetCoordinates(from), ShotDirection = direction.Normalized(), Gun = ent, Shooter = user, Target = target,
            };
            RaiseLocalEvent(ammo, ref hitscanEv);
        }
    }

    private void ShootProjectile(Entity<StellarGunTypesReloadableComponent> ent, Vector2 direction, EntityUid user)
    {
        var initialSpeed = Physics.GetMapLinearVelocity(user);
        if (ent.Comp.MultiShotAmount > 1)
        {
            var angles = LinearSpread(direction.ToAngle() - ent.Comp.MultiShotSpread / 2, direction.ToAngle() + ent.Comp.MultiShotSpread / 2, ent.Comp.MultiShotAmount);

            for (var i = 0; i < ent.Comp.MultiShotAmount; i++)
            {
                var angleWiggle = _random.NextAngle(ent.Comp.MultiShotWiggleMin, ent.Comp.MultiShotWiggleMax) + angles[i];
                var projectileEv = new StellarProjectileEvent(angleWiggle.ToVec(), initialSpeed, ent, user);
                RaiseLocalEvent(ent, ref projectileEv);
            }
        }
        else
        {
            var projectileEv = new StellarProjectileEvent(direction, initialSpeed, ent, user);
            RaiseLocalEvent(ent, ref projectileEv);
        }
    }

    private void OnHitscan(Entity<StellarGunHitscanComponent> ent, ref HitscanTraceEvent args)
    {
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
            ent.Comp.Ray);
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

    /// <summary>
    /// RSI containing the appropriate sprites for the hitscan- expecting "start", "middle", "end", and "bullet" states.
    /// </summary>
    public SpriteSpecifier.Rsi RayVisuals;

    public EntProtoId? MuzzleFlash;

    public StellarHitscanEvent(CollisionGroup collisionMask, NetCoordinates fromCoords, Vector2 shotDirection, NetEntity gun, NetEntity? shooter, NetEntity? target, bool unshaded, Color lightColor, float maxDist, SpriteSpecifier.Rsi rayVisuals)
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
        RayVisuals = rayVisuals;
    }
}

[ByRefEvent]
public record struct StellarProjectileEvent(Vector2 Direction, Vector2 InitialSpeed, Entity<StellarGunTypesReloadableComponent> Gun, EntityUid User);
