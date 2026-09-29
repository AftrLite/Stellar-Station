// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-CosmicCult

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.RecyclerChute;

/// <summary>
/// Marker component for marking where chutes send "traveling" objects inside the cinematic "Tube World" dimension when chutes flush their contents to a Recycler Telepad.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarChuteTravelMarkerComponent : Component
{

}
