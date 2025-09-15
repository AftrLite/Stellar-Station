// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: MIT

using Content.Shared._ST.Lobby;
using Content.Stellar.Shared.Lobby;
using Robust.Client.GameObjects;

namespace Content.Stellar.Client.Lobby;

public sealed class LobbyCharacterSystem : SharedLobbyCharacterSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LobbyCharacterComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<LobbyCharacterComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not { } sprite)
            return;

        if (_appearance.TryGetData<bool>(ent, LobbyCharacterVisuals.Visible, out var visible, args.Component))
        {
            var opacity = 1f;
            if (!visible)
                opacity = 0f;
            _sprite.SetColor((ent, sprite), sprite.Color.WithAlpha(opacity));
        }
    }
}
