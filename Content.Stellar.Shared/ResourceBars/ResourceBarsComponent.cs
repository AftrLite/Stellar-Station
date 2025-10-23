using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.ResourceBars;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
[Access(typeof(SharedResourceBarsSystem))]
public sealed partial class ResourceBarsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ResourceBarPrototype>, ResourceBarState> Bars = new();

    public override bool SendOnlyToOwner => true;
}

[DataDefinition, Serializable, NetSerializable]
public partial record struct ResourceBarState
{
    [DataField]
    public float Fill;
}
