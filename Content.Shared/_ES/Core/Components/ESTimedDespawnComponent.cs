// SPDX-FileCopyrightText: 2025 EmoGarbage404
// SPDX-FileCopyrightText: 2025 moonheart08
// SPDX-FileCopyrightText: 2025 mirrorcult
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Spawners;

namespace Content.Shared._ES.Core.Timer.Components;

/// <summary>
/// ES-specific version of <see cref="TimedDespawnComponent"/> with networking capabilities
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(ESTimedDespawnSystem), Other = AccessPermissions.Read)] // Stellar
public sealed partial class ESTimedDespawnComponent : Component
{
    /// <summary>
    /// How long the entity will exist before despawning
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Lifetime = TimeSpan.FromSeconds(1.25f); // Stellar - Giving this a default value is convenient

    /// <summary>
    /// The time at which the entity will despawn
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan DespawnTime;

    /// <summary>
    /// The time at which the entity spawned
    /// </summary>
    [ViewVariables]
    public TimeSpan SpawnTime => DespawnTime - Lifetime;
}

[Serializable, NetSerializable]
public enum ESTimedDespawnVisuals : byte
{
    DespawnTime,
}
