// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Shared._ST.Interaction;

/// <summary>
/// Marker grid attached to either Grids or Players to prevent construction from taking place.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BlockGridConstructionComponent : Component
{
    /// <summary>
    /// Blocks construction.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BlockConstruction = true;
}
