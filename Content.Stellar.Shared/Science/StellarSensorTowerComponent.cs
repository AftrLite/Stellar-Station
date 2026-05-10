// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// Component that defines an entity as a Sensor Tower, a structure for use in Science.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarSensorTowerComponent : Component
{
    [DataField, AutoNetworkedField] public StellarSensorTowerState State = StellarSensorTowerState.NoDrive;

    [DataField] public bool Damaged;

    [DataField] public float Progress;

    [DataField] public float ProgressGainMin = 16; // 18

    [DataField] public float ProgressGainMax = 33.34f; // 33.34

    [DataField] public TimeSpan ProcessingTimeMin = TimeSpan.FromSeconds(1); // 45

    [DataField] public TimeSpan ProcessingTimeMax = TimeSpan.FromSeconds(2); // 60

    [DataField]
    public Dictionary<StellarSensorTowerEffect, float> EffectProbabilities = new()
    {
        {StellarSensorTowerEffect.Nothing, 75f},
        {StellarSensorTowerEffect.Damage, 10f}, // Once the tower is damaged, future rolls of "Damage" result in "Desync".
        {StellarSensorTowerEffect.AttractMobs, 10f},
        {StellarSensorTowerEffect.Desync, 5f}, // This has a pseudo-weight of 15 if the condition described above is met.
    };

    [DataField] public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");

    [DataField] public SoundSpecifier? EjectSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagOut/revolver_magout.ogg");

    [DataField] public SoundSpecifier? SoundInput = new SoundPathSpecifier("/Audio/_ST/Machines/ringtone.ogg");

    [DataField] public SoundSpecifier? SoundActivate = new SoundPathSpecifier("/Audio/_ST/Machines/ringtone.ogg");

    [DataField] public SoundSpecifier? SoundSynchronize = new SoundPathSpecifier("/Audio/_ST/Machines/ringtone.ogg");

    [DataField] public EntProtoId FullDrive = "StellarScienceDataDriveFull";

    [DataField, AutoNetworkedField]
    public EntityUid? LinkedTerminal;
}

[Serializable, NetSerializable]
public enum StellarSensorTowerRadialKey
{
    Key,
}

[Serializable, NetSerializable]
public enum StellarSensorTowerEffect
{
    Nothing,
    Damage,
    AttractMobs,
    Desync,
}

[Serializable, NetSerializable]
public enum StellarSensorTowerState
{
    Idle,
    Ringing,
    Syncing,
    Processing,
    FullDrive,
    NoDrive,
}
