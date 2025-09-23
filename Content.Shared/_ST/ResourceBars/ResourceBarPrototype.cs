// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._ST.ResourceBars;

/// <summary>
/// A resource bar that a player has.
/// </summary>
[Prototype]
public sealed partial class ResourceBarPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of this resource bar.
    /// This field is neither required nor localized because it doesn't really matter, but having it is nice for organizing.
    /// </summary>
    [DataField]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// The tooltip text players will see when hovering their mouse over the resource bar UI element.
    /// This field is required because GAMEPLAY COMMUNICATION MATTERS, PEOPLE. IF IT'S ON THE UI, YOU'RE /TALKING/ TO THE PLAYER.
    /// </summary>
    [DataField(required: true)]
    public LocId Tooltip;

    /// <summary>
    /// The icon associated with the resource bar, displayed on the UI.
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Icon = SpriteSpecifier.Invalid;

    /// <summary>
    /// The UI position to place this resource bar. I wanted to make this an Enum, but i don't know how! Enums are bytes though, so it's basically the same thing. Lmao.
    /// Value must be 1, 2, or 3. 1 = Left, 2 = Middle, 3 = Right. It's that simple.
    /// </summary>
    [DataField(required: true)]
    public ResourceUIPosition? Location;

    /// <summary>
    /// This resource bar's color. I could've made this default to white, but instead, it's required!
    /// Why? Because if you're adding a new resource bar i expect you to put in a bare minimum effort! It's not much to ask!
    /// </summary>
    [DataField(required: true)]
    public Color? Color;

    [DataField]
    public ComponentRegistry? AssociatedComponents;
}

[Serializable, NetSerializable]
public enum ResourceUIPosition
{
    Left = 1,
    Middle = 2,
    Right = 3,
}
