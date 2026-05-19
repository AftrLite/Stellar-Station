// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// Component that defines an entity as an Anomaly.
/// Anomalies are a Station Hazard and are a part of the Science gameplay loop.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class StellarAnomalyComponent : Component
{
    /// <summary>
    /// Timer for the current "Integrity Pip".
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField, AutoNetworkedField]
    public TimeSpan? CurrentPipTimer;

    [DataField] public TimeSpan PipTimerMin = TimeSpan.FromSeconds(10); // 60

    [DataField] public TimeSpan PipTimerMax = TimeSpan.FromSeconds(20); // 240

    [DataField] public TimeSpan PipTimerStable = TimeSpan.FromSeconds(30); // 300

    /// <summary>
    /// The amount of "Integrity Pips" the anomaly has left. When this value decreases, the anomaly "Pulses". When this value reaches 0, the anomaly fizzles out and disappears.
    /// If this value is unset on spawn, it's initialized to the Max, modulated by SpawnVariance.
    /// </summary>
    [DataField] public int? IntegrityPipsValue;

    [DataField] public int IntegrityPipsMax = 5;

    [DataField] public int IntegrityPipsSpawnVariance = 2;

    [DataField] public float DestabilizeChance = 0.5f;

    [DataField, AutoNetworkedField] public bool Stable;

    [DataField] public EntProtoId SpawnVfx = "StellarExplosionAnomalySpawn";

    [DataField] public EntProtoId PulseVfx = "StellarExplosionAnomalyPulse";

    [DataField] public SoundSpecifier? SoundPulse;
}
