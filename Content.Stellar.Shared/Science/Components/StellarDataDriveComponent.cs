// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Science.Components;

/// <summary>
/// Component that defines an entity as a data drive. Used for Sensor Towers & Bluespace Drive.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarDataDriveComponent : Component
{
    /// <summary>
    /// Is the drive full?
    /// </summary>
    [DataField] public bool Full;
}

[Serializable, NetSerializable]
public enum StellarDataDriveVisuals
{
    Visuals,
}
