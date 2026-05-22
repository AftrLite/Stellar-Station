// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Client.UserInterface.Controls;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

/// <summary>
/// The radial menu used by Stellar Sensor Towers.
/// </summary>
[UsedImplicitly]
public sealed class StellarSensorTowerBoundUserInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu;

    private readonly ResPath _rsiPath = new("/Textures/_ST/Icons/radial-icons-sensortower.rsi");

    public StellarSensorTowerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (_menu?.IsOpen == true || !EntMan.HasComponent<StellarSensorTowerComponent>(Owner))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(GetButtons());
        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> GetButtons()
    {
        var options = new HashSet<RadialMenuOptionBase>();

        var symbolChi = new RadialMenuActionOption<int>(CodeMessage, 1)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "1")),
            ToolTip = "Chi",
            KeepOpen = true,
        };

        var symbolPsi = new RadialMenuActionOption<int>(CodeMessage, 2)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "2")),
            ToolTip = "Psi",
            KeepOpen = true,
        };

        var symbolXi = new RadialMenuActionOption<int>(CodeMessage, 3)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "3")),
            ToolTip = "Xi",
            KeepOpen = true,
        };

        var symbolPi = new RadialMenuActionOption<int>(CodeMessage, 4)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "4")),
            ToolTip = "Pi",
            KeepOpen = true,
        };

        var symbolTheta = new RadialMenuActionOption<int>(CodeMessage, 5)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "5")),
            ToolTip = "Theta",
            KeepOpen = true,
        };

        var symbolOmega = new RadialMenuActionOption<int>(CodeMessage, 6)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "6")),
            ToolTip = "Omega",
            KeepOpen = true,
        };

        var cancelOption = new RadialMenuActionOption<TowerMenuMethod>(StateMessage, TowerMenuMethod.Cancel)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "cancel")),
            ToolTip = "Cancel sync sequence",
        };

        options.Add(symbolXi);
        options.Add(symbolTheta);
        options.Add(symbolPsi);

        options.Add(cancelOption);

        options.Add(symbolChi);
        options.Add(symbolOmega);
        options.Add(symbolPi);

        return options;
    }

    private void CodeMessage(int digit)
    {
        var message = new StellarSensorTowerRadialMessage(TowerMenuMethod.CodeInput, digit);
        SendPredictedMessage(message);
    }

    private void StateMessage(TowerMenuMethod state)
    {
        var message = new StellarSensorTowerRadialMessage(state);
        SendPredictedMessage(message);
    }
}
