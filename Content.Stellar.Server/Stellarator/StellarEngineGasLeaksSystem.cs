// SPDX-FileCopyrightText: 2026 AftrLite
// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Linq;
using Content.Server.Atmos.EntitySystems;
using Content.Stellar.Shared.Stellarator;
using Robust.Shared.Random;

namespace Content.Stellar.Server.Stellarator;

public sealed class StellarEngineGasLeaksSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    private EntityQuery<StellarEngineGasLeakSourceComponent> _sourceQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarEngineGasLeaksComponent, StellarEngineChaosEvent>(OnChaosEvent);

        _sourceQuery = GetEntityQuery<StellarEngineGasLeakSourceComponent>();
    }

    private void OnChaosEvent(Entity<StellarEngineGasLeaksComponent> ent, ref StellarEngineChaosEvent args)
    {
        if (!TryComp<StellarEngineCoreComponent>(ent, out var core) || !_random.Prob(args.Intensity * ent.Comp.ChaosEventProbability))
            return;

        var target = _random.Pick(core.LinkedParts.Where(part => _sourceQuery.HasComponent(part)).ToList());
        var gas = _random.Pick(ent.Comp.ChaosEventGases);
        var mixture = _atmosphere.GetTileMixture(target, true);
        mixture?.AdjustMoles(gas, 200f);
    }
}

