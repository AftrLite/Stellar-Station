// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill <uhhadd@gmail.com>
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ST.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Stellar.Shared.ResourceBars;

namespace Content.Stellar.Shared.Mobs;

public abstract class SharedInternalsResourceBarSystem : EntitySystem
{
    [Dependency] private readonly SharedResourceBarsSystem _resourceBars = default!;
    [Dependency] private readonly SharedInternalsSystem _internals = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InternalsResourceBarComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<InternalsResourceBarComponent, InternalsToggledEvent>(OnInternalsToggled);
    }

    private void OnShutdown(Entity<InternalsResourceBarComponent> ent, ref ComponentShutdown args)
    {
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBar);
    }

    private void OnInternalsToggled(Entity<InternalsResourceBarComponent> ent, ref InternalsToggledEvent args)
    {
        UpdateBars(ent);
    }

    protected void UpdateBars(Entity<InternalsResourceBarComponent> ent)
    {
        if (!_internals.AreInternalsWorking(ent) || !TryComp<InternalsComponent>(ent, out var internals))
        {
            _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBar);
            return;
        }

        var gasTank = Comp<GasTankComponent>(internals.GasTankEntity!.Value);

        var expectedMaximumMoleage = (ent.Comp.MaxFillPressure * gasTank.Air.Volume) / (Atmospherics.R * gasTank.Air.Temperature);
        var currentMoleage = gasTank.Air.TotalMoles;
        var ratio = currentMoleage / expectedMaximumMoleage;

        _resourceBars.ShowResourceBar(ent.Owner, ent.Comp.ResourceBar, ratio);
    }
}