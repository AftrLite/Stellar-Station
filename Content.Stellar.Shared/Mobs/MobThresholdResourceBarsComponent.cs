using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Mobs;

[RegisterComponent]
[Access(typeof(MobThresholdResourceBarsSystem))]
public sealed partial class MobThresholdResourceBarsComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ResourceBarPrototype> ResourceBar;
}
