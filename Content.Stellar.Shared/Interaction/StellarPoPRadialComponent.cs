// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Interaction;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarPoPRadialComponent : Component
{
    /// <summary>
    /// Icon for constructing the options on the Stellar Player-on-Player interaction radial.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconPull = new(new ResPath("/Textures/_ST/Icons/interaction-radial-icons.rsi"), "pull");

    /// <inheritdoc cref="IconPull"/>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconStrip = new(new ResPath("/Textures/_ST/Icons/interaction-radial-icons.rsi"), "inspect");

    /// <inheritdoc cref="IconPull"/>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconExamine = new(new ResPath("/Textures/_ST/Icons/interaction-radial-icons.rsi"), "examine");

    /// <inheritdoc cref="IconPull"/>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconSocial = new(new ResPath("/Textures/_ST/Icons/interaction-radial-icons.rsi"), "social");

    /// <inheritdoc cref="IconPull"/>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi? IconOfferItem = new(new ResPath("/Textures/_ST/Icons/interaction-radial-icons.rsi"), "offer-item");

    /// <summary>
    /// DoAfter time for opening the Inspect menu (the "Strip" menu.) through the Stellar Interaction RadialMenu.
    /// </summary>
    [DataField] public TimeSpan InpsectTime = TimeSpan.FromSeconds(1);
}

[Serializable, NetSerializable]
public enum StellarPoPRadialKey
{
    Key,
}

[Serializable, NetSerializable]
public enum StellarPoPInteractionMethod
{
    Pull,
    Strip,
    Emote,
    OfferItem,
}
