// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.ResourceBars;

public abstract class SharedResourceBarsSystem : EntitySystem
{
    protected virtual void AfterUpdateBars(Entity<ResourceBarsComponent> ent)
    {
    }

    public void SetFill(Entity<ResourceBarsComponent?> ent, ProtoId<ResourceBarPrototype> barId, float fill)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!ent.Comp.Bars.TryGetValue(barId, out var bar))
            return;

        ent.Comp.Bars[barId] = bar with { Fill = fill };
        Dirty(ent);
        AfterUpdateBars((ent.Owner, ent.Comp));
    }
}
