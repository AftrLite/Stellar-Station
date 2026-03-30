// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Destructible.Thresholds;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarEngineTunerComponent : Component
{
    /// <summary>
    /// The state of the Tuner. Used by the Engine Core and appearance stuff.
    /// </summary>
    [DataField, AutoNetworkedField] public TunerState State;

    /// <summary>
    /// Whether the tuner is broken.
    /// </summary>
    [DataField, AutoNetworkedField] public bool TunerBroken;

    /// <summary>
    /// The current value to increment the Tuner's "calibration" by whenever a ContactPad DoAfter fires on it.
    /// </summary>
    [DataField] public float TunerIncrementCalibrationValue = 3.75f;

    [DataField] public float TunerIncrementCalibrationValueMin = 0.1f;

    [DataField] public float TunerIncrementCalibrationValueMax = 3.75f;

    /// <summary>
    /// How much Calibration Buildup this Tuner has.
    /// </summary>
    [DataField] public float CalibrationWiggleCurrentBuildup = 0f;

    /// <summary>
    /// The current Calibration of the Tuner.
    /// </summary>
    [DataField, AutoNetworkedField] public float TunerCurrentCalibration = 0;

    [DataField] public float TunerMaxValue = 100;

    [DataField] public float TunerMinValue = 0;

    /// <summary>
    /// The range wherein a Tuner is "Near-Calibrated".
    /// </summary>
    [DataField] public MinMax TunerCalibrationWindowYellow = new(50, 95); //  new(60, 95);

    /// <summary>
    /// The range wherein a Tuner is Fully Calibrated.
    /// </summary>
    [DataField] public MinMax TunerCalibrationWindowGreen = new(65, 80); // new(70, 83);

    [DataField] public SoundSpecifier CalibrateSfx = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    /// <summary>
    /// Chance to break on a chaos event
    /// </summary>
    [DataField]
    public float ChaosEventProbability = 0.5f;

    /// <summary>
    /// Multiplier for how much calibration decays by.
    /// Should always be a negative value in order for the math to work out.
    /// This shouldn't typically be modified, and passing a non-negative number will cause Calibration to build rather than decay.
    /// </summary>
    [DataField] public float CalibrationDecayMult = -1f;

    /// <summary>
    /// How much the Calibration will Decay by when Calibration Wiggle Buildup maxes out.
    /// </summary>
    [DataField] public float CalibrationWiggleDecayStrength;

    /// <summary>
    /// How much Calibration Buildup is needed to trigger Wiggle" Decay.
    /// </summary>
    [DataField] public float CalibrationWiggleBuildupNeeded;

    /// <summary>
    /// The chance for a Tuner's Calibration Wiggle to build up.
    /// Chance is randomized out of 0-100, so this is a straight % chance.
    /// Keep in mind that this ticks on every atmos device update.
    /// </summary>
    [DataField] public float CalibrationDecayBuildupChance;

    /// <summary>
    /// How quickly a Tuner's CalibrationWiggleCurrentBuildup increases by when its buildsup procs on a tick.
    /// </summary>
    [DataField] public float CalibrationWiggleBuildupSpeed;

    /// <summary>
    /// The amount by which a Tuner's Calibration decays every single Atmos Update.
    /// This value is capped to what a given Tuner's maximum Calibration gain is.
    /// </summary>
    [DataField] public float CalibrationPassiveDecay;

    /// <summary>
    /// The fixture of the contact pad.
    /// </summary>
    [DataField]
    public string ContactPadFixture = "contactPad";

    /// <summary>
    /// The Entity currently on the ContactPad, if any.
    /// </summary>
    [DataField, AutoNetworkedField]
    private EntityUid? _activeDoAfterUser;

    /// <summary>
    /// The do-after index, if any.
    /// </summary>
    [DataField, AutoNetworkedField]
    private ushort? _activeDoAfterIndex;

    public DoAfterId? ActiveDoAfterId
    {
        get =>
            _activeDoAfterUser.HasValue && _activeDoAfterIndex.HasValue
                ? new(_activeDoAfterUser.Value, _activeDoAfterIndex.Value)
                : null;

        set
        {
            _activeDoAfterUser = value?.Uid;
            _activeDoAfterIndex = value?.Index;
        }
    }

    /// <summary>
    /// Whether or not the Contact Pad is enabled.
    /// </summary>
    [DataField, AutoNetworkedField] public bool Enabled;

    /// <summary>
    /// Multiplier for the ContactPad's DoAfter speed.
    /// </summary>
    [DataField] public float DoAfterSpeedMult = 1f;

    /// <summary>
    /// Amount of time it takes for the ContactPad to run a DoAfter.
    /// </summary>
    [DataField] public float BaseTime = 0.625f;

}

[Serializable, NetSerializable]
public enum TunerVisuals
{
    Broken,
    Tuner,
    Venting,
}

[Serializable, NetSerializable]
public enum TunerLayers
{
    Base,
    Screen,
    Vents,
    Pistons,
}

[Serializable, NetSerializable]
public enum TunerState
{
    Off,
    NotCalibrated,
    NearCalibrated,
    Calibrated,
}

[Serializable, NetSerializable]
public enum TunerBaseState
{
    Broken,
    Intact,
}

[Serializable, NetSerializable]
public enum TunerVentState
{
    Inert,
    Venting,
}
