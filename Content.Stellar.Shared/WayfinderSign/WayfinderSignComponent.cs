// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.WayfinderSign;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WayfinderSignComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Slot1ID = "Slot1";

    [DataField, AutoNetworkedField]
    public string Slot2ID = "Slot2";

    [DataField, AutoNetworkedField]
    public string Slot3ID = "Slot3";

}
[Serializable, NetSerializable]
public enum WayfinderSignLayers : byte
{
    Slot1,
    Slot1Arrow,
    Slot2,
    Slot2Arrow,
    Slot3,
    Slot3Arrow,
}
