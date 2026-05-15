// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// Component that defines an entity as an Anomaly Containment Capsule. Used for Anomaly containment & Bluespace Drive.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarContainmentCapsuleComponent : Component
{
    /// <summary>
    /// Is the capsule full?
    /// </summary>
    [DataField] public bool Full;
}
