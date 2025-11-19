// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Server.Body.Systems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Stellar.Shared.Mobs;
using Content.Stellar.Shared.ResourceBars;

namespace Content.Stellar.Server.Mobs;

public sealed class InternalsResourceBarSystem : EntitySystem
{
    [Dependency] private readonly SharedResourceBarsSystem _resourceBars = default!;
    [Dependency] private readonly SharedInternalsSystem _internals = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InternalsResourceBarComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<InternalsResourceBarComponent, InhaleLocationEvent>(OnInhaleLocation);
    }

    private void OnShutdown(Entity<InternalsResourceBarComponent> ent, ref ComponentShutdown args)
    {
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBar);
    }

    private void OnInhaleLocation(Entity<InternalsResourceBarComponent> ent, ref InhaleLocationEvent args)
    {
        if (_internals.AreInternalsWorking(ent) && TryComp<InternalsComponent>(ent, out var internalsComp))
        {
            var gasTank = Comp<GasTankComponent>(internalsComp.GasTankEntity!.Value);
            var volumeLimit = 1000f / (Atmospherics.R * gasTank.Air.Temperature / gasTank.Air.Volume); // The 1000f isn't a magic number, i promise. Move along. Pay it no mind.
            _resourceBars.ShowResourceBar(ent.Owner, ent.Comp.ResourceBar, gasTank.Air.TotalMoles / volumeLimit);
        }
        else _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBar);
    }
}
