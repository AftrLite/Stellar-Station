// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.RecyclerChute;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class StellarRecyclerChuteComponent : Component
{
    /// <summary>
    /// Icons for constructing the options on the Stellar Interaction radial.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconActivate;// = new(new ResPath("/Textures/_ST/Icons/chute-radial-icons.rsi"), "activate");

    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconEject;// = new(new ResPath("/Textures/_ST/Icons/chute-radial-icons.rsi"), "eject");

    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconInsert;// = new(new ResPath("/Textures/_ST/Icons/chute-radial-icons.rsi"), "insert");

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? CooldownTimer;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? AutoActivateTimer;

    public DoAfterId? DoAfterId = null;

    [DataField, AutoNetworkedField] public ChuteState State = ChuteState.Idle;

    [DataField] public string ContainerId = "entity_storage";

    [DataField] public TimeSpan AutoActivateTime = TimeSpan.FromSeconds(15);

    [DataField] public TimeSpan CooldownTime = TimeSpan.FromSeconds(30);

    [DataField] public TimeSpan TravelTimeMin = TimeSpan.FromSeconds(10);

    [DataField] public TimeSpan TravelTimeMax = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Time it takes to enter the Recycler Chute ourselves.
    /// </summary>
    [DataField] public TimeSpan SelfEnterDoAfter = TimeSpan.FromSeconds(0.5f);

    /// <summary>
    /// Time it takes to push someone else into the Recycler Chute.
    /// </summary>
    [DataField] public TimeSpan OtherEnterDoAfter = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Time it takes to charge up.
    /// </summary>
    [DataField] public TimeSpan ChargeTime = TimeSpan.FromSeconds(3.5);

    /// <summary>
    /// The entity hosting the audio playback for the charge sound. Used for cancelling the charge DoAfter.
    /// </summary>
    [DataField] public EntityUid? ChargeAudioStream;

    /// <summary>
    /// Sound played when an object is throw into the container.
    /// </summary>
    [DataField] public SoundSpecifier? SoundInsert = new SoundPathSpecifier("/Audio/Effects/trashbag1.ogg");

    /// <summary>
    /// Sound played when an object is throw into the container.
    /// </summary>
    [DataField] public SoundSpecifier? SoundCharge = new SoundPathSpecifier("/Audio/_ST/CosmicCult/Abilities/ability-imposition.ogg");
}

[Serializable, NetSerializable]
public enum ChuteVisuals : byte
{
    Base,
}

[Serializable, NetSerializable]
public enum ChuteState : byte
{
    Idle,
    Charging,
    Cooldown,
}

[Serializable, NetSerializable]
public enum ChuteMenuMethod : byte
{
    Activate,
    Eject,
    Insert,
}

[Serializable, NetSerializable]
public enum StellarChuteRadialKey
{
    Key
}
