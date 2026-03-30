// SPDX-FileCopyrightText: 2026 AftrLite
// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Linq;
using Content.Shared._ES.TileFires;
using Robust.Shared.Random;

namespace Content.Stellar.Shared.Stellarator;

public sealed class StellarEngineFiresSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ESSharedTileFireSystem _tileFire = default!;

    private EntityQuery<StellarEngineFireSourceComponent> _sourceQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarEngineFiresComponent, StellarEngineChaosEvent>(OnChaosEvent);

        _sourceQuery = GetEntityQuery<StellarEngineFireSourceComponent>();
    }

    private void OnChaosEvent(Entity<StellarEngineFiresComponent> ent, ref StellarEngineChaosEvent args)
    {
        if (!TryComp<StellarEngineCoreComponent>(ent, out var core) || !_random.Prob(args.Intensity * ent.Comp.ChaosEventProbability))
            return;

        var target = _random.Pick(core.LinkedParts.Where(part => _sourceQuery.HasComponent(part)).ToList());
        _tileFire.TryDoTileFire(Transform(target).Coordinates, ent, 4);
    }
}
