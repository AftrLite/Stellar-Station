// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Anomaly;
using Content.Shared.Anomaly.Effects.Components;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Stellar.Shared.Science.AnomalyBehaviour;

public sealed class StellarAnomalySpawnSystem : EntitySystem
{
    [Dependency] private readonly SharedAnomalySystem _wizAnomaly = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyPulseSpawnComponent, StellarAnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<StellarAnomalyPulseSpawnComponent, MapInitEvent>(OnSpawn);
    }

    private void OnPulse(Entity<StellarAnomalyPulseSpawnComponent> ent, ref StellarAnomalyPulseEvent args)
    {
        foreach (var entry in ent.Comp.Entries)
        {
            SpawnEntities(ent, entry);
        }
    }

    private void OnSpawn(Entity<StellarAnomalyPulseSpawnComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.SpawnOnSpawn)
            return;
        foreach (var entry in ent.Comp.Entries)
        {
            SpawnEntities(ent, entry);
        }
    }

    private void SpawnEntities(Entity<StellarAnomalyPulseSpawnComponent> ent, EntitySpawnSettingsEntry entry)
    {
        var randomWeight = _random.NextFloat(0, 1.0f);
        var xform = Transform(ent);
        if (!TryComp(xform.GridUid, out MapGridComponent? grid))
            return;

        var tiles = _wizAnomaly.GetSpawningPoints(ent, randomWeight, randomWeight, entry.Settings, randomWeight);
        if (tiles == null)
            return;

        foreach (var tileref in tiles)
        {
            Spawn(_random.Pick(entry.Spawns), _mapSystem.ToCenterCoordinates(tileref, grid));
        }
    }
}
