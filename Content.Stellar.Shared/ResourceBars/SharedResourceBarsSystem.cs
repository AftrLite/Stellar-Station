// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.ResourceBars;

public abstract class SharedResourceBarsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    protected virtual void AfterUpdateBars(Entity<ResourceBarsComponent> ent)
    {
    }

    public void ClearResourceBar(Entity<ResourceBarsComponent?> ent, ProtoId<ResourceBarPrototype> barId)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!ent.Comp.Bars.Remove(barId))
            return;

        Dirty(ent);
        AfterUpdateBars((ent.Owner, ent.Comp));
    }

    public void ClearResourceBarCategory(Entity<ResourceBarsComponent?> ent, ProtoId<ResourceBarCategoryPrototype> category)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        foreach (var bar in ent.Comp.Bars.Keys)
        {
            if (_prototype.Index(bar).Category == category)
                ent.Comp.Bars.Remove(bar);
        }

        Dirty(ent);
        AfterUpdateBars((ent.Owner, ent.Comp));
    }

    public void ShowResourceBar(Entity<ResourceBarsComponent?> ent, ProtoId<ResourceBarPrototype> barId, float fill)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (ent.Comp.Bars.TryGetValue(barId, out var existingBar))
        {
            ent.Comp.Bars[barId] = existingBar with { Fill = fill };
            Dirty(ent);
            AfterUpdateBars((ent.Owner, ent.Comp));
            return;
        }

        var newCategory = _prototype.Index(barId).Category;
        foreach (var bar in ent.Comp.Bars.Keys)
        {
            if (_prototype.Index(bar).Category == newCategory)
                ent.Comp.Bars.Remove(bar);
        }

        ent.Comp.Bars[barId] = new ResourceBarState() { Fill = fill };

        Dirty(ent);
        AfterUpdateBars((ent.Owner, ent.Comp));
    }
}
