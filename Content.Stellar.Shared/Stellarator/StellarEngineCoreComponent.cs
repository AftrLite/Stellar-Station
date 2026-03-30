// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Atmos;
using Content.Stellar.Shared.Overcharge;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SharedStellarEngineSystem))]
public sealed partial class StellarEngineCoreComponent : Component
{
    #region Housekeeping
    /// <summary>
    /// All the parts that the Engine's core is linked to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> LinkedParts = new();

    /// <summary>
    /// The Engine's current state. Set to Available by default.
    /// </summary>
    [DataField, AutoNetworkedField] public EngineCoreState State = EngineCoreState.Idle;

    /// <summary>
    /// The range of the engine's screenshake.
    /// </summary>
    [DataField] public int ScreenshakeRange = 20;
    #endregion Housekeeping

    #region Timers
    /// <summary>
    /// If true, that means we're in the grace period for Overcharged or Hypercharged.
    /// </summary>
    [DataField] public bool GracePeriod;

    /// <summary>
    /// How much time the crew is guaranteed to have Overcharge before they must resume upkeep.
    /// </summary>
    [DataField] public TimeSpan GracePeriodOvercharged = TimeSpan.FromSeconds(10f); //90f

    /// <summary>
    /// How much time the crew is guaranteed to have Hypercharge before they must resume upkeep.
    /// </summary>
    [DataField] public TimeSpan GracePeriodHypercharged = TimeSpan.FromSeconds(10f); //150f

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan GraceTime;

    /// <summary>
    /// Time used to ramp the engine's decay to unsustainable amounts during Overcharge and Hypercharging.
    /// </summary>
    [DataField] public TimeSpan GameOverTimeCharged = TimeSpan.FromSeconds(300f); // 300f

    /// <summary>
    /// Time used to ramp the engine's decay to unsustainable amounts during Hypercharge.
    /// </summary>
    [DataField] public TimeSpan GameOverTimeHypercharged = TimeSpan.FromSeconds(150f); // 150f

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan GameOverTime;

    /// <summary>
    /// Wether or not chaos events can occur.
    /// </summary>
    [DataField] public bool ChaosEnabled;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan ChaosTime;

    #endregion

    #region Overcharge

    /// <summary>
    /// Wether or not the engine has activated an Overcharge. Reset when the engine shuts down.
    /// </summary>
    [DataField] public bool HasActivatedOvercharge;

    /// <summary>
    /// The overcharge that the engine can currently provide.
    /// </summary>
    [DataField] public StellarOverchargePrototype? CurrentOvercharge;

    /// <summary>
    /// The multiplier for the rate at which the engine gains Charge Progress.
    /// </summary>
    [DataField] public float ChargeGainMult = 0.4f;

    /// <summary>
    /// The multiplier for the rate at which the engine loses Charge Progress.
    /// </summary>
    [DataField] public float ChargeDecayMult = 0.3f;

    /// <summary>
    /// The current amount of Overcharge progress the engine has.
    /// </summary>
    [DataField] public float ChargeProgress = 0f;

    [DataField] public float ChargeMax = 100f;

    [DataField] public float ChargeMin = 0f;

    [DataField] public bool CanHypercharge;
    #endregion Overcharge

    #region Fuel
    /// <summary>
    /// Wether or not the engine has fuel.
    /// </summary>
    [DataField, AutoNetworkedField] public bool HasFuel;

    /// <summary>
    /// Wether or not the Engine is currently accepting fuel.
    /// </summary>
    [DataField, AutoNetworkedField] public bool AcceptingFuel = true;

    /// <summary>
    /// The rate at which the current fuel is used up.
    /// </summary>
    [DataField] public float FuelDecayRate = 0.5f;

    /// <summary>
    /// The multiplier for the rate at which the current fuel is used up.
    /// </summary>
    [DataField] public float FuelDecayMult = 1f;

    /// <summary>
    /// The current amount of Fuel the engine has.
    /// </summary>
    [DataField] public float FuelValue = 0f;

    [DataField] public float FuelMax = 100f;

    [DataField] public float FuelMin = 0f;
    #endregion Fuel
}
#region Enums

[Serializable, NetSerializable]
public enum EngineCoreVisuals
{
    Core,
}

[Serializable, NetSerializable]
public enum EngineCoreLayers
{
    ProgressLeft,
    ProgressRight,
    CoreVfx,
}

[Serializable, NetSerializable]
public enum EngineCoreState
{
    Idle,
    Unavailable, // For when the engine is being sabotaged.
    Cooldown,
    Overcharging,
    Overcharged,
    Hypercharging,
    Hypercharged,
}
#endregion Enums
