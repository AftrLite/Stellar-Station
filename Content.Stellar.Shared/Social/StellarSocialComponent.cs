// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Social;

/// <summary>
/// Component for housing Stellar Social Interaction
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class StellarSocialComponent : Component
{
    /// <summary>
    /// Timer for social cooldowns.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan SocialCooldown;

    /// <summary>
    /// Timeout timer for awaiting a response to a Social request.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? ResponseTimeout;

    /// <summary>
    /// Hashset for storing Social/Co-op emotes.
    /// </summary>
    [DataField, AutoNetworkedField] public HashSet<ProtoId<StellarCoopEmotePrototype>> CoopEmotesAvailable;


    /// <summary>
    /// If not-null, the currently requested co-op emote in a social request.
    /// </summary>
    [DataField, AutoNetworkedField] public ProtoId<StellarCoopEmotePrototype>? RequestedEmote;

    /// <summary>
    /// How long a player has to respond to a social request before it times out.
    /// The animations of the request's pop-up are %calculated based off of this timespan's TotalSeconds, so modifying TimeoutTime doesn't break anything.
    /// That said, i did hardcode some stuff, so don't make timeouts *longer* than 9 seconds. Okay? Or change the hardcoded values. That works too.
    /// </summary>
    [DataField] public TimeSpan TimeoutTime = TimeSpan.FromSeconds(9);

    /// <summary>
    /// The entity that'll be spawned when requesting a social interaction.
    /// </summary>
    [DataField] public EntProtoId RequestVfxEnt = "StellarSocialRequestEffect";

    /// <summary>
    /// The sound effect that plays when this entity makes a social request.
    /// </summary>
    [DataField] public SoundSpecifier RequestSfx = new SoundPathSpecifier("/Audio/_ST/Misc/emote-notification.ogg");

    /// <summary>
    /// The entity housing the sprites/visual stuff for the request popup above a player's head when requesting a social interaction.
    /// </summary>
    [DataField] public EntityUid? RequestEffect;

    /// <summary>
    /// The entity that a social interaction is being requested from.
    /// </summary>
    [DataField, AutoNetworkedField] public EntityUid? Target;

    /// <summary>
    /// The entity currently being offered by a GiveItem social event.
    /// </summary>
    [DataField, AutoNetworkedField] public EntityUid? OfferedItem;
}

[Serializable, NetSerializable]
public sealed class StellarSocialComponentState : ComponentState
{
    public readonly NetEntity? Target;
    public readonly NetEntity? RequestEffect;
    public readonly NetEntity? OfferedItem;
    public readonly ProtoId<StellarCoopEmotePrototype>? RequestedEmote;

    public StellarSocialComponentState(NetEntity? target, NetEntity? requestEffect, NetEntity? offeredItem, ProtoId<StellarCoopEmotePrototype>? requestedEmote)
    {
        Target = target;
        RequestEffect = requestEffect;
        OfferedItem = offeredItem;
        RequestedEmote = requestedEmote;
    }
}

[ByRefEvent]
public readonly record struct StellarSocialStateChangeEvent;
