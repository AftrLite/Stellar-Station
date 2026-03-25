// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared._ES.Camera;
using Content.Shared._ES.TileFires;
using Content.Shared._ST.Shockwave;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Trigger;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Explosion;

public sealed class SharedStellarExplosionsSystem : EntitySystem
{

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;
    [Dependency] private readonly ESSharedTileFireSystem _tileFire = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!; // Shouldn't InRangeUnobstructed and InRangeUnoccluded be in a generic helper method at this point?
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    private readonly HashSet<EntityUid> _damageEnts = new();
    private readonly HashSet<EntityUid> _physicsEnts = new();

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<StellarExplosiveComponent, TriggerEvent>(OnExplosiveTriggered);
        SubscribeLocalEvent<StellarShockwaveComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<StellarShockwaveComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.StartTime = _timing.CurTime;
    }

    private void OnExplosiveTriggered(Entity<StellarExplosiveComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != ent.Comp.TriggerKey || TerminatingOrDeleted(ent))
            return;

        SetupExplosion(ent);

        args.Handled = true;
    }

    private void SetupExplosion(Entity<StellarExplosiveComponent> ent)
    {
        // Yes, this means the explosion is spawned on the outermost container, regardless of how deep the source of it is nested.
        // Is that weird? Maybe. But I don't like messing with containers, so I'm sure this is fine.
        if (_container.IsEntityOrParentInContainer(ent) && _container.TryGetOuterContainer(ent, Transform(ent), out var container))
        {
            StellarExplosion(container.Owner, ent.Comp);
            if (ent.Comp.Knockback || ent.Comp.Knockdown)
                ExplodeUnobstructed(container.Owner, ent.Comp);
        }
        else
        {
            StellarExplosion(ent, ent.Comp);
            if (ent.Comp.Knockback || ent.Comp.Knockdown)
                ExplodeUnobstructed(ent, ent.Comp);
        }
    }

    private void StellarExplosion(EntityUid target, StellarExplosiveComponent comp)
    {
        if (TerminatingOrDeleted(target))
            return;

        var coords = Transform(target).Coordinates;
        var mapCoords = _transform.ToMapCoordinates(coords);
        var audioRange = comp.Range * 2f;
        var farAudioRange = comp.Range * 5f;
        var filter = Filter.Pvs(mapCoords).AddInRange(mapCoords, audioRange);
        var shakeFilter = Filter.Empty().AddInRange(mapCoords, farAudioRange);

        foreach (var player in shakeFilter.Recipients)
        {
            if (player.AttachedEntity == null)
                continue; // huh???
            var playerPos = _transform.GetMapCoordinates(Transform(player.AttachedEntity.Value));
            var distance = (playerPos.Position - mapCoords.Position).LengthSquared() / farAudioRange;
            distance = Math.Clamp(distance/farAudioRange, 0f, farAudioRange);

            var lerpedTrauma = MathHelper.Lerp(comp.ShakeIntensity, 0.75f, distance);
            var lerpedDecay = MathHelper.Lerp(5f, 3.5f, distance);
            var lerpedFrequency = MathHelper.Lerp(0.008f, 0.004f, distance);
            var shake = new ESScreenshakeParameters() { Trauma = lerpedTrauma, DecayRate = lerpedDecay, Frequency = lerpedFrequency};
            _shake.Screenshake(player.AttachedEntity.Value, shake, null);
        }

        if (_net.IsClient && _timing.IsFirstTimePredicted)
        {
            Spawn(comp.Explosion, mapCoords);
            _audio.PlayStatic(comp.Sound, filter, coords, true);
        }

        if (_net.IsServer)
        {
            var farFilter = Filter.Empty().AddInRange(mapCoords, farAudioRange).RemoveInRange(mapCoords, audioRange);
            _audio.PlayGlobal(comp.SoundFar, farFilter, true);

            if (comp.MaxShrapnel >= 1 && comp.ShrapnelEffects != null)
            {
                var shrapnelCount = _random.Next(comp.MinShrapnel, comp.MaxShrapnel);
                var segmentAngle = 360 / shrapnelCount;
                for (var i = 0; i < shrapnelCount; i++)
                {
                    var angleMin = segmentAngle * i;
                    var angleMax = segmentAngle * (i + 1);
                    var angle = Angle.FromDegrees(_random.Next(angleMin, angleMax));
                    var direction = angle.ToVec().Normalized() * 20;
                    var shrapnel = Spawn(_random.Pick(comp.ShrapnelEffects), Transform(target).Coordinates);
                    _transform.SetWorldRotation(shrapnel, direction.ToAngle());
                    _throw.TryThrow(shrapnel, direction, Math.Abs(comp.ShrapnelSpeed), doSpin: false, animated: false);
                }
            }
        }

        if (comp.SetFire)
            _tileFire.TryDoTileFire(target, stage: 4);
    }

    private void ExplodeUnobstructed(EntityUid target, StellarExplosiveComponent comp)
    {
        var speed = comp.KnockbackSpeed;
        var mapCoords = _transform.GetMapCoordinates(target);
        if (comp.Knockback)
        {
            _physicsEnts.Clear();
            _lookup.GetEntitiesInRange(mapCoords.MapId, mapCoords.Position, comp.Range, _physicsEnts, flags: LookupFlags.Dynamic | LookupFlags.Sundries);

            foreach (var targetEnt in _physicsEnts) // AftrLite, why not use _repulseAttract? | Because we want to use InRangeUnobstructed, which RepulseAttract doesn't include.
            {
                if (_whitelist.IsWhitelistFail(comp.Whitelist, targetEnt))
                    continue;

                if (!comp.IgnoreObstruction && !_interaction.InRangeUnobstructed(target, targetEnt, range: comp.Range))
                    continue;

                if (!_physicsQuery.TryGetComponent(targetEnt, out var physics)
                    || (physics.CollisionLayer & (int)CollisionGroup.GhostImpassable) != 0x0)
                    continue;

                var targetPos = _transform.GetWorldPosition(targetEnt);
                var direction = targetPos - mapCoords.Position;
                if (direction == Vector2.Zero)
                    continue;

                speed *= direction.Length();

                var throwDirection = speed < 0 ? -direction : direction.Normalized() * (comp.Range - direction.Length());

                _throw.TryThrow(targetEnt, throwDirection, Math.Abs(speed), recoil: false, compensateFriction: true);
            }
        }

        if (!_timing.IsFirstTimePredicted) // Do we need this here? I'm not sure, so i'll leave it commented out for now.
            return;

        _damageEnts.Clear();
        _lookup.GetEntitiesInRange(mapCoords.MapId, mapCoords.Position, comp.Range, _damageEnts);

        foreach (var targetEnt in _damageEnts)
        {
            if (_whitelist.IsWhitelistFail(comp.Whitelist, targetEnt))
                continue;

            if (comp.Knockdown)
                _stun.TryKnockdown(targetEnt, comp.KnockdownDuration);

            if (comp.Stun)
                _stun.TryAddStunDuration(targetEnt, comp.StunDuration);
        }
    }
}
