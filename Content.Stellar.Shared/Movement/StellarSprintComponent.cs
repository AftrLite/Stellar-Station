using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Movement;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(StellarSprintSystem))]
public sealed partial class StellarSprintComponent : Component
{
    /// <summary>
    /// Is this entity sprinting?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Sprinting = false;

    /// <summary>
    /// Maximum amount of sprint energy available.
    /// </summary>
    [DataField(required: true)]
    public float EnergyMax;

    /// <summary>
    /// Current sprint energy.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Energy = 0f;

    /// <summary>
    /// Amount of Energy needed to be able to sprint again.
    /// </summary>
    [DataField(required: true)]
    public float MinimumSprintEnergy;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan LastUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.5d);

    /// <summary>
    /// The resource bar used to indicate energy
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ResourceBarPrototype> ResourceBar;

    /// <summary>
    /// How much sprint energy it takes to move one unit (tile)
    /// </summary>
    [DataField(required: true)]
    public float DecayPerUnitMoved;

    /// <summary>
    /// Multiplier for energy decay.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DecayModifier = 1f;

    /// <summary>
    /// How much sprint energy restores per second.
    /// </summary>
    [DataField(required: true)]
    public float RegenerationPerSecond;

    /// <summary>
    /// Multiplier for energy regeneration.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RegenerationModifier = 1f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan RegenerationCooldown = default!;

    /// <summary>
    /// The amount of time after sprinting it takes to start regenerating
    /// </summary>
    [DataField(required: true)]
    public TimeSpan RegenerationInterval;
}
