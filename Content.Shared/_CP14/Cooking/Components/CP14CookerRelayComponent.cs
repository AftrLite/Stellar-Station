// SPDX-FileCopyrightText: 2025 AftrLite

//
// SPDX-License-Identifier: MIT

/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CP14.Cooking.Components;

/// <summary>
/// Relay for CP14FoodCooker when its CookingMethod is set to CookingMethod.Nested. Required for cookers inside of storage.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CP14SharedCookingSystem))]
public sealed partial class CP14CookerRelayComponent : Component
{
    [DataField]
    public string ContainerId = "storagebase";

    [DataField, AutoNetworkedField]
    public bool Powered = false;

    [DataField, AutoNetworkedField]
    public bool Closed = false;

    /// <summary>
    /// The sound played when this cooker finishes cooking an item inside it.
    /// </summary>
    [DataField]
    public SoundSpecifier? FinishSound;
}
