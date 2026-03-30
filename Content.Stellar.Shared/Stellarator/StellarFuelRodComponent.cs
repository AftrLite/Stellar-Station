// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.Overcharge;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedStellarEngineSystem))]
public sealed partial class StellarFuelRodComponent : Component
{
    /// <summary>
    /// The overcharge that this Fuel Rod provides.
    /// </summary>
    [DataField(required:true)] public ProtoId<StellarOverchargePrototype>? ProvidedOvercharge;

    /// <summary>
    /// Wether or not this fuel rod can allow the engine to Hypercharge.
    /// </summary>
    [DataField(required: true)] public bool CanHypercharge;

    /// <summary>
    /// The current amount of fuel this Fuel Rod provides. Values 1-100.
    /// </summary>
    [DataField] public float FuelProvided = 100f;

    /// <summary>
    /// The amount by which Tuners' Calibration decays every single Atmos Update.
    /// This value is capped to what a given Tuner's maximum Calibration gain is.
    /// </summary>
    [DataField] public float CalibrationPassiveDecay = 0.5f;

    /// <summary>
    /// How quickly Tuners' CalibrationWiggleCurrentBuildup increases by when its buildsup procs on a tick.
    /// </summary>
    [DataField] public float CalibrationWiggleBuildupSpeed = 1f;

    /// <summary>
    /// The chance for a Tuner's Calibration Wiggle to build up.
    /// Chance is randomized out of 0-100, so this is a straight % chance.
    /// Keep in mind that this ticks on every atmos device update.
    /// </summary>
    [DataField] public float CalibrationDecayBuildupChance = 0.225f; // 0.225 -> 22.5%

    /// <summary>
    /// How much Calibration Buildup is needed to trigger Wiggle Decay.
    /// </summary>
    [DataField] public float CalibrationWiggleBuildupNeeded = 15f;

    /// <summary>
    /// How much the Calibration will Decay by when Wiggle Buildup maxes out.
    /// </summary>
    [DataField] public float CalibrationWiggleDecayStrength = 10f;
}
