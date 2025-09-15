// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: MIT

using Content.Server.GameTicking;
using Content.Shared._ST.Lobby;
using Content.Shared.GameTicking;
using Content.Shared.Players;
using Content.Stellar.Shared.Lobby;
using Robust.Server.GameObjects;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Stellar.Server.Lobby;

public sealed class LobbyCharacterSystem : SharedLobbyCharacterSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        _playerManager.PlayerStatusChanged += OnPlayerChange;
        SubscribeLocalEvent<LobbyCharacterComponent, PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnPlayerChange(object? sender, SessionStatusEventArgs args)
    {
        ToggleLobbyCharacter(args.Session, args.NewStatus);
    }

    private void OnPlayerDetached(Entity<LobbyCharacterComponent> ent, ref PlayerDetachedEvent args)
    {
        ToggleLobbyCharacter(args.Player, args.Player.Status);
    }

    private void ToggleLobbyCharacter(ICommonSession session, SessionStatus sessionStatus)
    {
        if (session.ContentData() is not { } data || data.LobbyEntity is null)
            return;

        var lobbyCharacter = data.LobbyEntity.Value;
        _ticker.PlayerGameStatuses.TryGetValue(session.UserId, out var playerStatus);

        if (sessionStatus == SessionStatus.Disconnected)
            _appearance.SetData(lobbyCharacter, LobbyCharacterVisuals.Visible, false);
        else if (playerStatus == PlayerGameStatus.JoinedGame || playerStatus == PlayerGameStatus.Observing)
            _appearance.SetData(lobbyCharacter, LobbyCharacterVisuals.Visible, false);
        else _appearance.SetData(lobbyCharacter, LobbyCharacterVisuals.Visible, true);
    }
}
