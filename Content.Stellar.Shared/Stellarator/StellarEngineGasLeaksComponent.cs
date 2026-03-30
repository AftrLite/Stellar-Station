// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent]
public sealed partial class StellarEngineGasLeaksComponent : Component
{
    /// <summary>
    /// Gases that can be picked to be leaked during a chaos event.
    /// </summary>
    [DataField]
    public Gas[] ChaosEventGases =
    {
        Gas.Plasma,
        Gas.Tritium,
        Gas.WaterVapor,
    };

    [DataField]
    public float ChaosEventProbability = 0.2f;
}
