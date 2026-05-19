// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ES.Camera;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Hitscan.Events;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Science;

public abstract class SharedStellarScienceAnomalySystem : EntitySystem
{
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyComponent, MapInitEvent>(OnAnomalyInit);
        SubscribeLocalEvent<StellarStabilizerBeamComponent, HitscanRaycastFiredEvent>(OnHitscanHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shuntQuery = EntityQueryEnumerator<StellarAnomalyComponent>();
        while (shuntQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.CurrentPipTimer is { } pipTimer && _timing.CurTime >= pipTimer)
            {
                comp.Stable = false; // You let the timer run out on its own! Suffer destabilization!
                DecrementPips((uid, comp));
                PulseAnomaly((uid, comp));
                Dirty(uid, comp);
            }
        }
    }

    private void OnHitscanHit(Entity<StellarStabilizerBeamComponent> ent, ref HitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity == null)
            return;

        var hitEnt = args.Data.HitEntity;
        RaiseNetworkEvent(new StellarAnomalyReactionVisualsEvent(GetNetEntity(hitEnt.Value), GetNetEntity(args.Data.Gun)));
        _stun.TryAddParalyzeDuration(hitEnt.Value, TimeSpan.FromSeconds(1.5f));
        _stun.TrySeeingStars(hitEnt.Value);
    }

    private void OnAnomalyInit(Entity<StellarAnomalyComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.IntegrityPipsValue is null)
            ent.Comp.IntegrityPipsValue = ent.Comp.IntegrityPipsMax - _random.Next(0, ent.Comp.IntegrityPipsSpawnVariance);

        _shake.LerpedShake(ent, 0.75f, 0.75f, 0.0085f, 40f);
        ent.Comp.CurrentPipTimer = _timing.CurTime + _random.Next(ent.Comp.PipTimerMin, ent.Comp.PipTimerMax);
        PredictedSpawnAtPosition(ent.Comp.SpawnVfx, Transform(ent).Coordinates);
        Dirty(ent);
    }

    private void StabilizeAnomaly(Entity<StellarAnomalyComponent> ent)
    {
        ent.Comp.CurrentPipTimer = _timing.CurTime + ent.Comp.PipTimerStable;
        ent.Comp.Stable = true;
        Dirty(ent);
    }

    private void PulseAnomaly(Entity<StellarAnomalyComponent> ent)
    {
        PredictedSpawnAtPosition(ent.Comp.PulseVfx, Transform(ent).Coordinates);
        _shake.LerpedShake(ent, 0.75f, 0.75f, 0.0085f, 40f);
    }

    private void DecrementPips(Entity<StellarAnomalyComponent> ent)
    {
        ent.Comp.CurrentPipTimer = _timing.CurTime + _random.Next(ent.Comp.PipTimerMin, ent.Comp.PipTimerMax);
        ent.Comp.IntegrityPipsValue--;
        if (ent.Comp.IntegrityPipsValue <= 0)
            PredictedQueueDel(ent);
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarAnomalyReactionVisualsEvent : EntityEventArgs
{
    public NetEntity Target;

    public NetEntity Source;

    public StellarAnomalyReactionVisualsEvent(NetEntity target, NetEntity source)
    {
        Target = target;
        Source = source;
    }
}
