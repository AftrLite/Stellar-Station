// SPDX-FileCopyrightText: 2026 AftrLite
// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Random;

namespace Content.Stellar.Shared.Stellarator;

public sealed class StellarEngineWiresSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarEngineWiresComponent, StellarEngineChaosEvent>(OnChaosEvent);
    }

    private void OnChaosEvent(Entity<StellarEngineWiresComponent> ent, ref StellarEngineChaosEvent args)
    {
        if (!_random.Prob(args.Intensity * ent.Comp.ChaosEventProbability))
            return;

        Spawn(ent.Comp.ActiveWires, Transform(ent).Coordinates);
    }
}
