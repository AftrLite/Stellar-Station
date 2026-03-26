// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Station;
using Content.Stellar.Shared.Overcharge.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Overcharge;

public sealed class StellarOverchargeSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private readonly HashSet<Entity<StellarOverchargeableComponent>> _overchargeSet = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarOverchargeableComponent, StellarToggleOverchargeEvent>(OnToggleOvercharge);
    }

    public void ToggleOvercharge(EntityUid source, ProtoId<StellarOverchargePrototype>? overchargeType, OverchargeState state)
    {
        DebugTools.Assert(overchargeType.HasValue == (state != OverchargeState.Disabled));
        var stationEnt = _station.GetOwningStation(source);
        if (stationEnt is null)
            return;

        var gridUid = _station.GetLargestGrid(stationEnt.Value);
        if (!HasComp<MapGridComponent>(gridUid))
            return;

        var stationGrid = gridUid.Value;

        _overchargeSet.Clear();
        _lookup.GetChildEntities(stationGrid, _overchargeSet);
        if (TryComp<StellarOverchargeableComponent>(stationEnt, out var overchargeComp))
            _overchargeSet.Add((stationEnt.Value, overchargeComp)); // The station itself can be a valid overcharge target too, so add it to the hashset, as GetChildEntities will probably skip it.

        foreach (var ent in _overchargeSet)
        {
            if (ent.Comp.RequiredOvercharge != overchargeType)
            {
                var evt = new StellarToggleOverchargeEvent(OverchargeState.Disabled);
                RaiseLocalEvent(ent, ref evt, true);
            }
            else
            {
                var evt = new StellarToggleOverchargeEvent(state);
                RaiseLocalEvent(ent, ref evt, true);
            }
        }
    }

    private void OnToggleOvercharge(Entity<StellarOverchargeableComponent> ent, ref StellarToggleOverchargeEvent args)
    {
        ent.Comp.State = args.State;
        Dirty(ent);

        _appearance.SetData(ent, OverchargeVisuals.Visuals, args.State);
    }
}

/// <summary>
/// Raised on entities when they're being overcharge-toggled.
/// </summary>
/// <param name="State">The new overcharge state.</param>
[ByRefEvent]
public readonly record struct StellarToggleOverchargeEvent(OverchargeState State);
