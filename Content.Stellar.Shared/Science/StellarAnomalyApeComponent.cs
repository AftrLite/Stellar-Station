// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// Component that defines an entity as an A.P.E. Used for Anomaly containment.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarAnomalyApeComponent : Component
{

}

[Serializable, NetSerializable]
public enum StellarApeRadialKey
{
    Key,
}
