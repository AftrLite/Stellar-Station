using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._ST.Movement;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), AutoGenerateComponentPause]
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
    [DataField, AutoNetworkedField]
    public float EnergyMax = 100f;

    /// <summary>
    /// Minimum sprint energy.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float EnergyMin = 0f;

    /// <summary>
    /// Current sprint energy.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Energy = 0f;

    /// <summary>
    /// Percentage of EnergyMax needed to be able to sprint again.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Resprint = 0.2f;

    /// <summary>
    /// To avoid continuously updating our data we track the last time we updated so we can extrapolate our current stamina.
    /// </summary>
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]

    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    /// How much sprint energy reduces per tick.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Decay = 2f;

    /// <summary>
    /// Multiplier for energy decay.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DecayMod = 1f;

    /// <summary>
    /// How much sprint energy restores per tick.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Regen = 3f;

    /// <summary>
    /// Multiplier for energy regen.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RegenMod = 1f;

    /// <summary>
    /// Multiplier for energy regen.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MoveModifier = 1f;


    /// <summary>
    /// How long until sprint energy starts regenerating.
    /// </summary>
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan RegenCooldown = default!;

    /// <summary>
    /// The amount of time between regen ticks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan RegenWait = TimeSpan.FromSeconds(2.5);

}
