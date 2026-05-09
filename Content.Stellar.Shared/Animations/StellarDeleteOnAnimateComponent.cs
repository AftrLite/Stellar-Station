// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Animations;

/// <summary>
/// Marker component for entities who should be automatically deleted when they complete an animation.
/// Useful for visual effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarDeleteOnAnimateComponent : Component
{
    [DataField] public bool DeleteOnStop;
}
