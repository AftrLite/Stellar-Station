// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-CosmicCult

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.RecyclerChute;

/// <summary>
/// Marker component for stations to enable Stellar Recycler Chutes functionality on them.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarRecyclerChuteStationComponent : Component;
