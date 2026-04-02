// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Weapons;

/// <summary>
/// This component lives on the hitscans
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarGunProjectileComponent : Component
{
    [DataField] public float ProjectileSpeed = 25f;
}
