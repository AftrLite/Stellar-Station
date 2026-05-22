// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Science.Components;

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
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? CurrentPipTimer;

    [DataField] public TimeSpan PipTimeMin = TimeSpan.FromSeconds(60); // 60

    [DataField] public TimeSpan PipTimeMax = TimeSpan.FromSeconds(200); // 200

    [DataField] public TimeSpan PipTimeStable = TimeSpan.FromSeconds(300);

    /// <summary>
    /// The amount of "Integrity Pips" the anomaly has left. When this value decreases, the anomaly "Pulses". When this value reaches 0, the anomaly fizzles out and disappears.
    /// If this value is unset on spawn, it's initialized to the Max, modulated by SpawnVariance.
    /// </summary>
    [DataField, AutoNetworkedField] public int? IntegrityPipsValue;

    [DataField] public int IntegrityPipsMax = 5;

    [DataField] public int IntegrityPipsSpawnVariance = 2;

    [DataField] public float DestabilizeChance = 0.5f;

    [DataField] public float TriggerChance;

    [DataField, AutoNetworkedField] public int CodeLength = 3;

    [DataField, AutoNetworkedField] public string EnteredCode = string.Empty;

    [DataField, AutoNetworkedField] public string AnomalyCode = string.Empty;

    [DataField, AutoNetworkedField] public bool Stable;

    [DataField] public EntProtoId CodePopup = "StellarEffectAnomalyCodePopup";

    [DataField] public EntProtoId SpawnVfx = "StellarExplosionAnomalySpawn";

    [DataField] public EntProtoId PulseVfx = "StellarExplosionAnomalyPulse";

    [DataField] public SoundSpecifier? SoundSpawn;

    [DataField] public SoundSpecifier? SoundPulse;
}
public enum AnomalyPopupVisuals : byte
{
    Key,
}
