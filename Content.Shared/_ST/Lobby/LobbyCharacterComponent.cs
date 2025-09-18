// SPDX-FileCopyrightText: 2025 AftrLite / Mirrorcult / Emogarbage
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._ST.Lobby;

/// <summary>
/// designates an entity as one of the special player-controlled lobby entities
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LobbyCharacterComponent : Component
{
}

[Serializable, NetSerializable]
public enum LobbyCharacterVisuals
{
    Visible,
}
