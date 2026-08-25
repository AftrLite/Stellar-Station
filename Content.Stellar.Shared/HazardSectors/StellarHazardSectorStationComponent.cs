// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Storage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.HazardSectors;

/// <summary>
/// Component for use in Hazard Sectors.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarHazardSectorStationComponent : Component
{
    [DataField] public Dictionary<EntProtoId, float> SectorMobs = new();
}
