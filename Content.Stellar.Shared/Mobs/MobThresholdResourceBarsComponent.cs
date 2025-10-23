// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: MIT

using Content.Shared.Mobs;
using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Mobs;

[RegisterComponent]
[Access(typeof(MobThresholdResourceBarsSystem))]
public sealed partial class MobThresholdResourceBarsComponent : Component
{
    [DataField(required: true)]
    public Dictionary<MobState, ProtoId<ResourceBarPrototype>> ResourceBars;

    [DataField(required: true)]
    public ProtoId<ResourceBarCategoryPrototype> ResourceBarCategory;
}
