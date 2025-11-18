// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Mobs;

[RegisterComponent, NetworkedComponent]
public sealed partial class InternalsResourceBarComponent : Component
{
    public ProtoId<ResourceBarPrototype> ResourceBar = "InternalsBar";
}
