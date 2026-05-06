// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Social;

[Prototype]
public sealed partial class StellarCoopEmotePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of this co-op emote. Used when populating the StellarRadialMenu.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// The text that'll pop up when this emote is requested.
    /// </summary>
    [DataField(required: true)]
    public LocId PopUpRequest;

    /// <summary>
    /// The text that pops up for each participant of the co-op emote.
    /// </summary>
    [DataField(required: true)]
    public LocId PopUpSuccess;

    /// <summary>
    /// Per-emote VFX entity support rather than constructing the VFX from a SpriteSpecifier.
    /// This allows supporting additional logic (e.g. rare variants of emotes) in the future more easily.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId VfxEntity;

    /// <summary>
    /// The icon associated with this co-op emote. Used for constructing the request popup visuals.
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier.Rsi? Icon;

    /// <summary>
    /// The sound that plays when this co-op emote is performed by its participants.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier? EmoteSound;
}
