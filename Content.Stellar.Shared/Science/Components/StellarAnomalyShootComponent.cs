// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.Weapons;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Science.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StellarAnomalyShootComponent : Component
{
    [DataField] public StellarGunMethod ShootingMethod = StellarGunMethod.Hitscan;

    [DataField] public EntProtoId Shootable;

    [DataField] public float ProjectileSpeed = 5;

    [DataField] public int ShootAmountMin = 1;

    [DataField] public int ShootAmountMax = 2;
}
