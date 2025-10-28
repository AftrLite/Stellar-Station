// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.ResourceBars;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
[Access(typeof(SharedResourceBarsSystem))]
public sealed partial class ResourceBarsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ResourceBarPrototype>, ResourceBarState> Bars = new();

    private List<ProtoId<ResourceBarCategoryPrototype>> _order;
    private readonly Dictionary<ProtoId<ResourceBarCategoryPrototype>, int> _collation = new();

    public IReadOnlyDictionary<ProtoId<ResourceBarCategoryPrototype>, int> Collation => _collation;

    [DataField(required: true)]
    public List<ProtoId<ResourceBarCategoryPrototype>> Order
    {
        get => _order;
        set
        {
            var i = 0;

            foreach (var item in value)
            {
                _collation[item] = i++;
            }

            _order = value;
        }
    }

    public override bool SendOnlyToOwner => true;
}

[DataDefinition, Serializable, NetSerializable]
public partial record struct ResourceBarState
{
    [DataField]
    public float Fill;
}
