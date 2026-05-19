// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Client.UserInterface.Controls;
using Content.Stellar.Server.CosmicCult.Components;
using Content.Stellar.Shared.Science;
using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

/// <summary>
/// The radial menu used by Stellar A.P.Es.
/// </summary>
[UsedImplicitly]
public sealed class StellarAbeBoundUserInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu;
    private readonly ResPath _rsiPath = new("/Textures/_ST/Icons/radial-icons-abe.rsi");

    public StellarAbeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (_menu?.IsOpen == true || !EntMan.HasComponent<StellarAnomalyAbeComponent>(Owner))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(GetButtons());
        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> GetButtons()
    {
        var options = new HashSet<RadialMenuOptionBase>();

        var pull = new RadialMenuActionOption<AbeMenuMethod>(StateMessage, AbeMenuMethod.Pull)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "pull")),
            ToolTip = "Pull",
        };

        var anchor = new RadialMenuActionOption<AbeMenuMethod>(StateMessage, AbeMenuMethod.Deploy)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "anchor")),
            ToolTip = "Anchor",
        };

        var rotate = new RadialMenuActionOption<AbeMenuMethod>(StateMessage, AbeMenuMethod.Rotate)
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

        var selectAlpha = new RadialMenuActionOption<EntProtoId>(ShootMessage, "StellarHitscanScienceAlpha")
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "alpha")),
            ToolTip = "Alpha",
        };

        var selectBeta = new RadialMenuActionOption<EntProtoId>(ShootMessage, "StellarHitscanScienceBeta")
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "beta")),
            ToolTip = "Beta",
        };

        var selectGamma = new RadialMenuActionOption<EntProtoId>(ShootMessage, "StellarHitscanScienceGamma")
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "gamma")),
            ToolTip = "Gamma",
        };

        var selectSigma = new RadialMenuActionOption<EntProtoId>(ShootMessage, "StellarHitscanScienceSigma")
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "sigma")),
            ToolTip = "Sigma",
        };

        var selectLambda = new RadialMenuActionOption<EntProtoId>(ShootMessage, "StellarHitscanScienceLambda")
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "lambda")),
            ToolTip = "Lambda",
        };

        shootNest.Add(selectAlpha);
        shootNest.Add(selectBeta);
        // shootNest.Add(selectLambda);
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

    private void ShootMessage(EntProtoId beamType)
    {
        var message = new StellarAbeRadialMessage(AbeMenuMethod.ShootBeam, beamType);
        SendPredictedMessage(message);
    }

    private void StateMessage(AbeMenuMethod state)
    {
        var message = new StellarAbeRadialMessage(state);
        SendPredictedMessage(message);
    }
}
