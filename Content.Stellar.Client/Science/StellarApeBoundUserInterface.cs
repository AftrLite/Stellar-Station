// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Client.UserInterface.Controls;
using Content.Stellar.Shared.Science;
using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

/// <summary>
/// The radial menu used by Stellar A.P.Es.
/// </summary>
[UsedImplicitly]
public sealed class StellarApeBoundUserInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu;
    private readonly ResPath _rsiPath = new("/Textures/_ST/Icons/radial-icons-ape.rsi");

    public StellarApeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (_menu?.IsOpen == true || !EntMan.HasComponent<StellarAnomalyApeComponent>(Owner))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(GetButtons());
        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> GetButtons()
    {
        var options = new HashSet<RadialMenuOptionBase>();

        var pull = new RadialMenuActionOption<ApeMenuMethod>(StateMessage, ApeMenuMethod.Pull)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "pull")),
            ToolTip = "Pull",
        };

        var anchor = new RadialMenuActionOption<ApeMenuMethod>(StateMessage, ApeMenuMethod.Anchor)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "anchor")),
            ToolTip = "Anchor",
        };

        var rotate = new RadialMenuActionOption<ApeMenuMethod>(StateMessage, ApeMenuMethod.Rotate)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "rotate")),
            ToolTip = "Rotate",
            KeepOpen = true,
        };

        var shootNest = new List<RadialMenuOptionBase>();
        var shoot = new RadialMenuNestedLayerOption(shootNest, 75f)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "beam")),
            ToolTip = "Stabilizer Beam",
        };

        var selectAlpha = new RadialMenuActionOption<ApeMenuMethod>(StateMessage, ApeMenuMethod.ShootAlpha)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "alpha")),
            ToolTip = "Alpha",
        };

        var selectBeta = new RadialMenuActionOption<ApeMenuMethod>(StateMessage, ApeMenuMethod.ShootBeta)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "beta")),
            ToolTip = "Beta",
        };

        var selectGamma = new RadialMenuActionOption<ApeMenuMethod>(StateMessage, ApeMenuMethod.ShootGamma)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "gamma")),
            ToolTip = "Gamma",
        };

        var selectSigma = new RadialMenuActionOption<ApeMenuMethod>(StateMessage, ApeMenuMethod.ShootSigma)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "sigma")),
            ToolTip = "Sigma",
        };
        shootNest.Add(selectAlpha);
        shootNest.Add(selectBeta);
        shootNest.Add(selectGamma);
        shootNest.Add(selectSigma);

        options.Add(anchor);

        if (EntMan.TryGetComponent<TransformComponent>(Owner, out var transform) && !transform.Anchored)
        {
            options.Add(pull);
        }
        else if (transform is not null && transform.Anchored)
        {
            options.Add(rotate);
            options.Add(shoot);
        }

        return options;
    }

    private void StateMessage(ApeMenuMethod state)
    {
        var message = new StellarApeRadialMessage(state);
        SendPredictedMessage(message);
    }
}
