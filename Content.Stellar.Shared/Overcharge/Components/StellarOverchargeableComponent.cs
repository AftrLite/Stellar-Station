// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Overcharge.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarOverchargeableComponent : Component
{
    /// <summary>
    /// The overcharge that interacts with this object.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<StellarOverchargePrototype>? RequiredOvercharge;

    /// <summary>
    /// The current overcharge state of this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public OverchargeState State = OverchargeState.Disabled;
}

[Serializable, NetSerializable]
public enum OverchargeVisuals : byte
{
    Visuals,
}

[Serializable, NetSerializable]
public enum OverchargeLayers : byte
{
    Layer1,
}

[Serializable, NetSerializable]
public enum OverchargeState : byte
{
    Disabled,
    Overcharged,
    Hypercharged,
}
