// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.Overcharge;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent]
[Access(typeof(StellarEngineInputSystem))]
public sealed partial class StellarEngineInputComponent : Component
{
    /// <summary>
    /// The timer used for the Input Module's door closing animation. The actual animation takes around 1.125 seconds, but since the final frame is buffered to take 99 seconds, it doesn't matter.
    /// </summary>
    [DataField]
    public TimeSpan DoorAnimTime = TimeSpan.FromSeconds(1.5);

    [DataField]
    public Dictionary<ProtoId<StellarOverchargePrototype>, EngineInputDisplayState> States = new()
    {
        { "StellarOverchargeOffensive", EngineInputDisplayState.Offensive },
        { "StellarOverchargeDefensive", EngineInputDisplayState.Defensive },
        { "StellarOverchargeCurative", EngineInputDisplayState.Curative },
        { "StellarOverchargeScientific", EngineInputDisplayState.Scientific },
    };

    /// <summary>
    /// The prototype ID for the depleted fuel rod the engine spits out whenever it runs out of fuel.
    /// </summary>
    [DataField] public EntProtoId DepletedFuel = "StellarFuelRodDepleted";
}

[Serializable, NetSerializable]
public enum EngineInputVisuals
{
    EngineInput,
    EngineDisplay,
}

[Serializable, NetSerializable]
public enum EngineInputLayers
{
    InputDoors,
    OverchargeDisplay,
}

[Serializable, NetSerializable]
public enum EngineInputDoorState
{
    Opening,
    Open,
    Closing,
    Closed,
}

[Serializable, NetSerializable]
public enum EngineInputDisplayState
{
    Off,
    Offensive,
    Defensive,
    Curative,
    Scientific,
}

