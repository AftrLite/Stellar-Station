// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// Component that defines an entity as a Sensor Tower, a structure for use in Science.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarSensorTowerComponent : Component
{
    [DataField, AutoNetworkedField] public StellarSensorTowerState State = StellarSensorTowerState.Idle;

    [DataField] public SoundSpecifier? SoundInput = new SoundPathSpecifier("/Audio/_ST/Machines/ringtone.ogg");

    [DataField] public SoundSpecifier? SoundActivate = new SoundPathSpecifier("/Audio/_ST/Machines/ringtone.ogg");

    [DataField] public SoundSpecifier? SoundSynchronize = new SoundPathSpecifier("/Audio/_ST/Machines/ringtone.ogg");

    [DataField] public float Progress;

    [DataField] public float ProgressGainMin = 20;

    [DataField] public float ProgressGainMax = 34;

    [DataField] public TimeSpan ProcessingTimeMin = TimeSpan.FromSeconds(45);

    [DataField] public TimeSpan ProcessingTimeMax = TimeSpan.FromSeconds(60);
}

[Serializable, NetSerializable]
public enum StellarSensorTowerRadialKey
{
    Key,
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
