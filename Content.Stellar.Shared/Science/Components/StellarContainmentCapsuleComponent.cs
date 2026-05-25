// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Science.Components;

/// <summary>
/// Component that defines an entity as an Anomaly Containment Capsule. Used for Anomaly containment & Bluespace Drive.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarContainmentCapsuleComponent : Component
{
    /// <summary>
    /// Is the capsule full?
    /// </summary>
    [DataField, AutoNetworkedField] public bool Full;

    /// <summary>
    /// How much anomalous energy the capsule is storing.
    /// </summary>
    [DataField] public int StoredEnergy;

    /// <summary>
    /// How much durability the capsule has left.
    /// </summary>
    [DataField] public int Durability;

    /// <summary>
    /// How long it takes to draw an anomaly into the containment capsule.
    /// </summary>
    [DataField] public TimeSpan ContainmentTime = TimeSpan.FromSeconds(10);
}

[Serializable, NetSerializable]
public enum StellarCapsuleVisuals
{
    Visuals,
}
