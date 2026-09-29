// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Client.UserInterface.Controls;
using Content.Stellar.Shared.RecyclerChute;
using Robust.Client.UserInterface;
using JetBrains.Annotations;

namespace Content.Stellar.Client.RecyclerChute;

/// <summary>
/// The radial menu used by Stellar Recycler Chutes.
/// </summary>
[UsedImplicitly]
public sealed class StellarRecyclerChuteBoundUserInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu;

    public StellarRecyclerChuteBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (_menu?.IsOpen == true || !EntMan.TryGetComponent<StellarRecyclerChuteComponent>(Owner, out var radialComp))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(GetButtons(Owner, radialComp));
        _menu.OpenOverMouseScreenPosition();
    }

    private IEnumerable<RadialMenuOptionBase> GetButtons(EntityUid ent, StellarRecyclerChuteComponent radialComp)
    {
        var options = new HashSet<RadialMenuOptionBase>();

        var activateOption = new RadialMenuActionOption<EntityUid>(ActivateMessage, ent)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconActivate),
            ToolTip = "Activate chute",
        };
        options.Add(activateOption);

        var jettisonOption = new RadialMenuActionOption<EntityUid>(EjectMessage, ent)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconEject),
            ToolTip = "Eject contents",
        };
        options.Add(jettisonOption);

        var insertSelfOption = new RadialMenuActionOption<EntityUid>(InsertSelfMessage, ent)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconInsert),
            ToolTip = "Enter chute",
        };

        options.Add(insertSelfOption);

        return options;
    }

    private void ActivateMessage(EntityUid target)
    {
        var message = new StellarChuteRadialMessage(EntMan.GetNetEntity(target), ChuteMenuMethod.Activate);
        SendPredictedMessage(message);
    }

    private void EjectMessage(EntityUid target)
    {
        var message = new StellarChuteRadialMessage(EntMan.GetNetEntity(target), ChuteMenuMethod.Eject);
        SendPredictedMessage(message);
    }

    private void InsertSelfMessage(EntityUid target)
    {
        var message = new StellarChuteRadialMessage(EntMan.GetNetEntity(target), ChuteMenuMethod.Insert);
        SendPredictedMessage(message);
    }
}
