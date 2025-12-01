// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.WayfinderSign;


/// <summary>
/// Component that designates an item as a label for a Wayfinder Sign.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(WayfinderSignSystem))]
public sealed partial class WayfinderLabelComponent : Component
{
    /// <summary>
    /// The base sprite for the label
    /// </summary>
    [DataField(required: true), AlwaysPushInheritance]
    public SpriteSpecifier BaseSprite;

    /// <summary>
    /// The arrow sprite for the label
    /// </summary>
    [DataField(required: true), AlwaysPushInheritance]
    public SpriteSpecifier ArrowSprite;

    /// <summary>
    /// The rotation of the arrow.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Direction ArrowRotation;
}

[Serializable, NetSerializable]
public enum WayfinderLabelVisuals : byte
{
    Base,
    Arrow,
    ArrowRotation,
}
