// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Atmos;
using Content.Stellar.Shared.ResourceBars;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Mobs;

[RegisterComponent, NetworkedComponent]
public sealed partial class InternalsResourceBarComponent : Component
{
    [DataField]
    public ProtoId<ResourceBarPrototype> ResourceBar = "InternalsBar";

    /// <summary>
    /// The expected maximum fill pressure used for computing how filled a gas tank is
    /// </summary>
    [DataField]
    public float MaxFillPressure = Atmospherics.OneAtmosphere * 10;
}
