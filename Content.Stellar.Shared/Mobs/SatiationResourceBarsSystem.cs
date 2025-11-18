// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Stellar.Shared.Mobs;
using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.Timing;

namespace Content.Stellar.Server.Mobs;

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

        var query = EntityQueryEnumerator<SatiationResourceBarsComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.LastUpdate + comp.UpdateInterval < _timing.CurTime)
                continue;

            comp.LastUpdate = _timing.CurTime;
            Dirty(uid, comp);

            UpdateResourceBars((uid, comp));
        }
    }

    private void OnMapInit(Entity<SatiationResourceBarsComponent> ent, ref MapInitEvent args)
    {
        UpdateResourceBars(ent);
    }

    private void UpdateResourceBars(Entity<SatiationResourceBarsComponent> ent)
    {
        if (TryComp<HungerComponent>(ent, out var hungerComp))
        {
            _resourceBars.ShowResourceBar(ent.Owner, ent.Comp.ResourceBarHunger, _hunger.GetHunger(hungerComp) / hungerComp.Thresholds[HungerThreshold.Okay]);
        }
        else
            _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBarHunger);

        if (TryComp<ThirstComponent>(ent, out var thirstComp))
        {
            _resourceBars.ShowResourceBar(ent.Owner, ent.Comp.ResourceBarThirst, thirstComp.CurrentThirst / thirstComp.ThirstThresholds[ThirstThreshold.Okay]);
        }
        else
            _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBarThirst);
    }

    private void OnShutdown(Entity<SatiationResourceBarsComponent> ent, ref ComponentShutdown args)
    {
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBarHunger);
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBarThirst);
    }
}
