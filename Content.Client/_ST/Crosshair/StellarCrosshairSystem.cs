// SPDX-FileCopyrightText: 2025 DrSmugLeaf | RMC-14
//
// SPDX-License-Identifier: MIT

using Content.Client.CombatMode;
using Content.Client.Hands.Systems;
using Content.Shared._ST.Crosshair;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client._ST.Crosshair;

public sealed class StellarCrosshairSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly CombatModeSystem _combatMode = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly SharedStellarCrosshairSystem _crosshair = default!;

    private bool _crosshairsEnabled;
    private ICursor? _crosshairCursor;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_config, CCVars.CombatModeIndicatorsPointShow, v => _crosshairsEnabled = v, true);
    }

    public override void FrameUpdate(float frameTime)
    {
        if (_ui.CurrentlyHovered is not IViewportControl)
            return;

        if (!_crosshairsEnabled || !_combatMode.IsInCombatMode())
        {
            _ui.CurrentlyHovered.CustomCursorShape = null;
            return;
        }

        var held = _hands.GetActiveHandEntity();

        if (held == null || _crosshair.GetCrosshair(held.Value) == null)
        {
            _ui.CurrentlyHovered.CustomCursorShape = null;
            return;
        }

        _crosshairCursor ??= _clyde.CreateCursor(new Image<Rgba32>(64, 64), Vector2i.One);
        _ui.CurrentlyHovered.CustomCursorShape = _crosshairCursor;
    }
}
