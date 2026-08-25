// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Animations;

/// <summary>
/// Component that plays a projectile-style animation on an entity from the moment it spawns.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarLifetimeAnimationComponent : Component
{
    [DataField] public string AnimateKey = "projectile-animation";

    [DataField] public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField] public bool UseTimeDespawn = true;
}
