// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

namespace Content.Stellar.Server.Science.Events;

/// <summary>
/// Used for spawning an Anomaly somewhere aboard the station.
/// The AnomalySpawnRule handles what anomaly to spawn, which is derived from the currently active hazard sector.
/// </summary>
[RegisterComponent]
public sealed partial class StellarAnomalySpawnRuleComponent : Component;
