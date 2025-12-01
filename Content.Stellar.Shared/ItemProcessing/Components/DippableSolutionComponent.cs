// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.ItemProcessing.Components;

/// <summary>
/// Component that allows DippableItems to interact with solutions on a given item.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedSolutionDippingSstem))]
public sealed partial class DippableSolutionComponent : Component
{
    /// <summary>
    /// Solution name that can be dipped in.
    /// </summary>
    [DataField]
    public string Solution = "default";

}
