// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Stellar.Shared.Mobs;
using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Mobs;

public sealed class SatiationResourceBarsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedResourceBarsSystem _resourceBars = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SatiationResourceBarsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SatiationResourceBarsComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SatiationResourceBarsComponent, HungerComponent, ThirstComponent>();

        while (query.MoveNext(out var uid, out var satiation, out var hunger, out var thirst))
        {
            if (satiation.LastUpdate + satiation.UpdateInterval < _timing.CurTime)
                continue;

            satiation.LastUpdate = _timing.CurTime;
            Dirty(uid, satiation);

            UpdateResourceBars((uid, satiation, hunger, thirst));
        }
    }

    private void OnMapInit(Entity<SatiationResourceBarsComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<HungerComponent>(ent, out var hunger))
            return;

        if (!TryComp<ThirstComponent>(ent, out var thirst))
            return;

        UpdateResourceBars((ent.Owner, ent.Comp, hunger, thirst));
    }

    private void UpdateResourceBars(Entity<SatiationResourceBarsComponent, HungerComponent, ThirstComponent> ent)
    {
        _resourceBars.ShowResourceBar(ent.Owner, ent.Comp1.ResourceBarHunger, _hunger.GetHunger(ent.Comp2) / ent.Comp2.Thresholds[HungerThreshold.Okay]);
        _resourceBars.ShowResourceBar(ent.Owner, ent.Comp1.ResourceBarThirst, ent.Comp3.CurrentThirst / ent.Comp3.ThirstThresholds[ThirstThreshold.Okay]);
    }

    private void OnShutdown(Entity<SatiationResourceBarsComponent> ent, ref ComponentShutdown args)
    {
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBarHunger);
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBarThirst);
    }
}
