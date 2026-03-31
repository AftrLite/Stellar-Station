// SPDX-FileCopyrightText: 2025 EmoGarbage404
// SPDX-FileCopyrightText: 2025 mirrorcult
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._ES.TileFires;

/// <summary>
///     Handles growth behavior for tile fires, as well as things like requiring oxygen.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class ESTileFireComponent : Component
{
    /// <summary>
    ///     Prototype to spawn when spreading.
    /// </summary>
    [DataField]
    public EntProtoId Prototype = "StellarTileFire";

    [DataField]
    public float MinFirestacksToSpread = 10;

    [DataField]
    public float FirestacksRemoveOnSpread = 3;

    [DataField]
    public float BaseSpreadChance = 0.66f;

    [DataField]
    public float MinimumOxyMolesToSpread = 3.6f; // 0.5f -> 3.6f. Less realistic fire feels more responsive and more fun.

    /// <summary>
    ///     Minimum time after the fire spawns at which it will smolder (return to first stage and stop spreading)
    /// </summary>
    [DataField]
    public TimeSpan MinSmolderTime = TimeSpan.FromMinutes(14);

    /// <summary>
    ///     Maximum time after the fire spawns at which it will smolder, see <see cref="MinSmolderTime"/>
    /// </summary>
    [DataField]
    public TimeSpan MaxSmolderTime = TimeSpan.FromMinutes(17);

    /// <summary>
    ///     Time chosen for this fire to smolder, using <see cref="MinSmolderTime"/> and <see cref="MaxSmolderTime"/>.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan SmolderTime;
}
