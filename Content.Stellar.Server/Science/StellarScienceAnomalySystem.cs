// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Audio;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;

namespace Content.Stellar.Server.Science;

public sealed class StellarScienceAnomalySystem : SharedStellarScienceAnomalySystem
{
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyComponent, MapInitEvent>(OnAnomalyInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shuntQuery = EntityQueryEnumerator<StellarAnomalyComponent>();
        while (shuntQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.CurrentPipTimer is { } pipTimer && Timing.CurTime >= pipTimer)
            {
                var ev1 = new StellarAnomalyDecrementEvent();
                RaiseLocalEvent(uid, ref ev1);
                if (comp.IntegrityPipsValue > 0)
                {
                    var ev3 = new StellarAnomalyDestabilizeEvent();
                    RaiseLocalEvent(uid, ref ev3);
                    MakeAnomalyPulse((uid, comp));
                }
            }
        }
    }

    private void OnAnomalyInit(Entity<StellarAnomalyComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.IntegrityPipsValue is null)
            ent.Comp.IntegrityPipsValue = ent.Comp.IntegrityPipsMax - Random.Next(0, ent.Comp.IntegrityPipsSpawnVariance);

        ent.Comp.CurrentPipTimer = Timing.CurTime + Random.Next(ent.Comp.PipTimeMin, ent.Comp.PipTimeMax);
        Timer.SpawnMethodTimer(TimeSpan.FromSeconds(1.2), () => PredictedSpawnAtPosition(ent.Comp.SpawnVfx, Transform(ent).Coordinates));
        Timer.SpawnMethodTimer(TimeSpan.FromSeconds(1.2), () => Shake.LerpedShake(ent, 1f, 0.75f, 0.0085f, 50f)); // Yucky timers! Used to sync up our audiovisual sauce.
        Timer.SpawnMethodTimer(TimeSpan.FromSeconds(1.3), () => _ambient.SetAmbience(ent, true));
        Timer.SpawnMethodTimer(TimeSpan.FromSeconds(1.3), () => _light.SetEnabled(ent, true));
        Audio.PlayPredicted(ent.Comp.SoundSpawn, ent, ent);

        ent.Comp.AnomalyCode = AnomalyCode(ent);
        Dirty(ent);
    }
}
