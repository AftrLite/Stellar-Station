// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Content.Stellar.Shared.HazardSectors;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Stellar.Server.HazardSectors;

public abstract class StellarGameRuleSystem<TComp> : GameRuleSystem<TComp> where TComp : IComponent
{
    [Dependency] protected readonly GameTicker Ticker = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public StellarShiftIntensityPrototype GetShiftIntensity()
    {
        return _prototype.Index(GetGameRule<StellarShiftIntensityRuleComponent>().Comp1.ShiftIntensityPreset);
    }

    protected TimeSpan ExpectedRoundEnd()
    {
        var rule = GetGameRule<StellarMajorThreatRuleComponent>();
        return rule.Comp1.ExpectedRoundEndTime;
    }

    protected TimeSpan ExpectedRoundDuration()
    {
        var rule = GetGameRule<StellarMajorThreatRuleComponent>();
        return rule.Comp1.ExpectedRoundEndTime - rule.Comp2.ActivatedAt;
    }

    protected Entity<T, GameRuleComponent> GetGameRule<T>() where T : IComponent
    {
        var query = EntityQueryEnumerator<T, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var comp, out var rule))
        {
            if (!Ticker.IsGameRuleActive(uid, rule))
                continue;

            return (uid, comp, rule);
        }

        throw new InvalidOperationException($"No game rule with the component {typeof(T).Name} exists");
    }
}
