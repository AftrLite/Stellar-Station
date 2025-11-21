// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.WayfinderSign;

[RegisterComponent, NetworkedComponent]
public sealed partial class WayfinderSignComponent : Component
{
    [DataField]
    public Dictionary<string, WayfinderSignSlot> Slots = new()
    {
        ["slot1"] = new(WayfinderSignLayers.Slot1Base, WayfinderSignLayers.Slot1Arrow, WayfinderSignLayers.Slot1Direction),
        ["slot2"] = new(WayfinderSignLayers.Slot2Base, WayfinderSignLayers.Slot2Arrow, WayfinderSignLayers.Slot2Direction),
        ["slot3"] = new(WayfinderSignLayers.Slot3Base, WayfinderSignLayers.Slot3Arrow, WayfinderSignLayers.Slot3Direction),
    };
}

[DataDefinition, NetSerializable, Serializable]
public partial struct WayfinderSignSlotsAppearance
{
    [DataField]
    public Dictionary<string, WayfinderSignSlot> Slots;

    public WayfinderSignSlotsAppearance(Dictionary<string, WayfinderSignSlot> slots)
    {
        Slots = slots;
    }
}

[DataDefinition, NetSerializable, Serializable]
public partial struct WayfinderSignSlot
{
    [DataField]
    public Enum Base;

    [DataField]
    public Enum Arrow;

    [DataField]
    public Enum Direction;

    public WayfinderSignSlot(Enum @base, Enum arrow, Enum direction)
    {
        Base = @base;
        Arrow = arrow;
        Direction = direction;
    }
};


[Serializable, NetSerializable]
public enum WayfinderSignLayers : byte
{
    Slots,

    Slot1Base,
    Slot1Arrow,
    Slot1Direction,
    Slot2Base,
    Slot2Arrow,
    Slot2Direction,
    Slot3Base,
    Slot3Arrow,
    Slot3Direction,
}
