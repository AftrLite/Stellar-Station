// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent]
public sealed partial class StellarEngineWiresComponent : Component
{
    /// <summary>
    /// The wires to spawn when the chaos event succeeds
    /// </summary>
    [DataField]
    public EntProtoId ActiveWires = "StellarEngineCablesElectrifiedOverlay";

	[DataField]
	public float ChaosEventProbability = 0.5f;
}
