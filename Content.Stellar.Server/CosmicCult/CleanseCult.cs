// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
//
// SPDX-License-Identifier: LicenseRef-CosmicCult

using Content.Stellar.Shared.CosmicCult.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Server.CosmicCult;

public sealed partial class CleanseCult : EntityEffectBase<CleanseCult>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-cleanse-cultist", ("chance", Probability));
    }
}

public sealed partial class CleanseCultEntityEffectSystem : EntityEffectSystem<CosmicCultComponent, CleanseCult>
{
    protected override void Effect(Entity<CosmicCultComponent> ent, ref EntityEffectEvent<CleanseCult> args)
    {
        EnsureComp<CleanseCultComponent>(ent); // We just slap them with the component and let the Deconversion system handle the rest.
    }
}
