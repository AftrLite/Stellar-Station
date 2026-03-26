// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Overcharge.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StellarOverchargeableTurretComponent : Component
{
    [DataField]
    public Dictionary<OverchargeState, EntProtoId> OverchargeStates = new()
    {
        { OverchargeState.Overcharged, "BulletEnergyTurretDisabler" },
        { OverchargeState.Hypercharged, "BulletLaserSpread" },
    };
}
