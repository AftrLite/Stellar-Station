// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.WayfinderSign;


/// <summary>
/// Component that designates an item as a label for a Wayfinder Sign.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WayfinderLabelComponent : Component
{
    /// <summary>
    /// The type of label this is. Must have matching sprite files in the given RSI (e.g. "atmos" or "sec").
    /// </summary>
    [DataField]
    public string LabelType = default!;

    /// <summary>
    /// What direction the arrow of the label is pointing in. 0 is South, 1 is North, 2 is East, and 3 is West.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int LabelDirection = 0;
}

[Serializable, NetSerializable]
public enum WayfinderLabelVisuals : byte
{
    Label,
}
