// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.ItemProcessing.Components;

/// <summary>
/// Component that allows an item to be transformed into another item by interacting with a valid DippingSolutionComponent.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSolutionDippingSstem))]
public sealed partial class DippableItemComponent : Component
{
    /// <summary>
    /// format YML like so:
    ///
    ///   reagentAndResult:
    ///     Milk: ClothingNeckScarfStripedSyndieGreen
    ///     Plasma: ClothingHeadHelmetHardsuitSyndieCommander
    ///
    /// </summary>
    [DataField(required: true)]
    public Dictionary<ProtoId<ReagentPrototype>, EntProtoId> ReagentAndResult { get; set; } = new();

    [DataField(required: true)]
    public FixedPoint2 NeededVolume;

    [DataField]
    public TimeSpan DippingTime = TimeSpan.FromSeconds(5);
    public Entity<SolutionComponent> CurrentSolution;
}
