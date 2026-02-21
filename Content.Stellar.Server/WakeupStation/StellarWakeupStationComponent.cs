// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

namespace Content.Stellar.Server.WakeupStation;

/// <summary>
/// Component for use in waking up stations.
/// </summary>
[RegisterComponent]
public sealed partial class StellarWakeupStationComponent : Component
{
    /// <summary>
    /// The currently active Station Grid EntityUid.
    /// </summary>
    public EntityUid? GridUid;
}
