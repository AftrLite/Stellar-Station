// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// Component that defines an entity as the Reality Anchor.
/// The reality anchor "allows" for anomalies to spawn while it's turned off. It's just a thematically-inverse anomaly generator.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarRealityAnchorComponent : Component
{
    [DataField] public bool Enabled = true;
}
