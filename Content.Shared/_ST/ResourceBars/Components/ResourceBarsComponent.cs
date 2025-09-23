using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ST.ResourceBars.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ResourceBarsComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<ResourceBarPrototype>> ActiveResourceBars = [];
}
