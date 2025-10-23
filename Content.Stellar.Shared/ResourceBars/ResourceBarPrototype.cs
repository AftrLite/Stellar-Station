// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.ResourceBars;

/// <summary>
/// A resource bar that a player has.
/// </summary>
[Prototype]
public sealed partial class ResourceBarPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

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
    /// The UI position to place this resource bar.
    /// </summary>
    [DataField(required: true)]
    public ResourceUIPosition Location;

    /// <summary>
    /// The resource bar's colour.
    /// </summary>
    [DataField(required: true)]
    public Color Color;
}

[Serializable, NetSerializable]
public enum ResourceUIPosition : byte
{
    Left = 1,
    Middle = 2,
    Right = 3,
}
