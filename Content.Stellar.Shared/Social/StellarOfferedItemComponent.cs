// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Social;

/// <summary>
/// Marker component for items being offered by a social interaction.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarOfferedItemComponent : Component;
