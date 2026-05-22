// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Science.Components;

/// <summary>
/// Component for marking a Hitscan Prototype as a Stabilizer Beam.
/// Stabilizer Beams are used in conjunction with Anomalies as a part of the Science gameplay loop.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarStabilizerBeamComponent : Component
{
    [DataField] public int Digit;
}
