// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Server.Mobs;
using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Mobs;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(SatiationResourceBarsSystem))]
public sealed partial class SatiationResourceBarsComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan LastUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1d);

    public ProtoId<ResourceBarPrototype> ResourceBarHunger = "HungerBar";

    public ProtoId<ResourceBarPrototype> ResourceBarThirst = "ThirstBar";
}
