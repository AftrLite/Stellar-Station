// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Client.UserInterface.Controls;
using Content.Stellar.Server.CosmicCult.Components;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Robust.Shared.Collections;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

/// <summary>
/// The radial menu used by Stellar A.P.Es.
/// </summary>
[UsedImplicitly]
public sealed class StellarSciPortablesBoundUserInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu;
    private readonly ResPath _rsiPath = new("/Textures/_ST/Icons/radial-icons-portables.rsi");

    public StellarSciPortablesBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (_menu?.IsOpen == true || !EntMan.TryGetComponent<StellarSciencePortablesComponent>(Owner, out var portableComp))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(GetButtons(portableComp));
        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> GetButtons(StellarSciencePortablesComponent comp)
    {
        var options = new HashSet<RadialMenuOptionBase>();

        var pull = new RadialMenuActionOption<SciPortableMenuMethod>(StateMessage, SciPortableMenuMethod.Pull)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "pull")),
            ToolTip = "Pull",
        };

        var anchor = new RadialMenuActionOption<SciPortableMenuMethod>(StateMessage, SciPortableMenuMethod.Deploy)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "anchor")),
            ToolTip = "Anchor",
        };

        var rotate = new RadialMenuActionOption<SciPortableMenuMethod>(StateMessage, SciPortableMenuMethod.Rotate)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "rotate")),
            ToolTip = "Rotate",
            KeepOpen = true,
        };

        var harvest = new RadialMenuActionOption<SciPortableMenuMethod>(StateMessage, SciPortableMenuMethod.Harvest)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "harvest")),
            ToolTip = "Harvest",
        };

        var shootNest = new List<RadialMenuOptionBase>();
        var shoot = new RadialMenuNestedLayerOption(shootNest, 75f)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "beam")),
            ToolTip = "Stabilizer Beam",
        };

        var selectAlpha = new RadialMenuActionOption<AbeBeamType>(ShootMessage, AbeBeamType.Alpha)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "1")),
            ToolTip = "Alpha",
        };

        var selectBeta = new RadialMenuActionOption<AbeBeamType>(ShootMessage, AbeBeamType.Beta)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "2")),
            ToolTip = "Beta",
        };

        var selectGamma = new RadialMenuActionOption<AbeBeamType>(ShootMessage, AbeBeamType.Gamma)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "3")),
            ToolTip = "Gamma",
        };

        var selectLambda = new RadialMenuActionOption<AbeBeamType>(ShootMessage, AbeBeamType.Lambda)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(new SpriteSpecifier.Rsi(_rsiPath, "lambda")),
            ToolTip = "Lambda",
        };

        shootNest.Add(selectAlpha);
        shootNest.Add(selectGamma);
        shootNest.Add(selectBeta);
        // shootNest.Add(selectLambda);

        options.Add(anchor);

        if (EntMan.TryGetComponent<TransformComponent>(Owner, out var transform) && !transform.Anchored)
        {
            options.Add(pull);
        }
        else if (transform is not null && transform.Anchored)
        {
            options.Add(rotate);
            switch (comp.PortablesType) // Switch for in case we add more Portables with semi-unique behavior.
            {
                case StellarPortablesType.Abe:
                    options.Add(shoot);
                    return options;
                case StellarPortablesType.Bea:
                    options.Add(harvest);
                    return options;
            }
        }

        return options;
    }

    private void ShootMessage(AbeBeamType beamType)
    {
        EntProtoId beam = "";
        EntProtoId muzzle = "";
        switch (beamType)
        {
            case AbeBeamType.Alpha:
                beam = "StellarHitscanScienceAlpha";
                muzzle = "StellarMuzzleFlashScienceAlpha";
                break;
            case AbeBeamType.Beta:
                beam = "StellarHitscanScienceBeta";
                muzzle = "StellarMuzzleFlashScienceBeta";
                break;
            case AbeBeamType.Gamma:
                beam = "StellarHitscanScienceGamma";
                muzzle = "StellarMuzzleFlashScienceGamma";
                break;
            case AbeBeamType.Lambda:
                break;
        }

        var message = new StellarSciRadialMessage(SciPortableMenuMethod.ShootBeam, beam, muzzle);
        SendPredictedMessage(message);
    }

    private void StateMessage(SciPortableMenuMethod state)
    {
        var message = new StellarSciRadialMessage(state);
        SendPredictedMessage(message);
    }

    private enum AbeBeamType : byte
    {
        Alpha,
        Beta,
        Gamma,
        Lambda,
    }
}
