// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Client.Examine;
using Content.Client.UserInterface.Controls;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Strip.Components;
using Content.Stellar.Shared.Social;
using Content.Stellar.Shared.Interaction;
using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Stellar.Client.Interaction.UI;

/// <summary>
/// The Stellar Station Player-on-Player interaction radial.
/// Used for Inspecting (stripMenu), Left-click pulling, Left-click examines, Item offers, and Social (Co-op) emotes, with the latter two backboned by the StellarSocialSystem.
/// </summary>
[UsedImplicitly]
public sealed class StellarPoPRadialBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private SimpleRadialMenu? _menu;

    public StellarPoPRadialBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        if (_menu?.IsOpen == true || !EntMan.TryGetComponent<StellarPoPRadialComponent>(Owner, out var radialComp))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);
        _menu.SetButtons(GetButtons(Owner, radialComp));
        _menu.OpenOverMouseScreenPosition();
    }

    private HashSet<RadialMenuOptionBase> GetButtons(EntityUid ent, StellarPoPRadialComponent radialComp)
    {
        var options = new HashSet<RadialMenuOptionBase>();

        if (EntMan.HasComponent<PullableComponent>(ent))
        {
            var pullOption = new RadialMenuActionOption<EntityUid>(SendPullMessage, ent)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconPull),
                ToolTip = "Pull",
            };
            options.Add(pullOption);
        }

        if (EntMan.HasComponent<StrippableComponent>(ent))
        {
            var stripOption = new RadialMenuActionOption<EntityUid>(SendStripMessage, ent)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconStrip),
                ToolTip = "Inspect",
            };
            options.Add(stripOption);
        }

        var examineOption = new RadialMenuActionOption<EntityUid>(SendExamineMessage, ent)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconExamine),
            ToolTip = "Examine",
        };
        options.Add(examineOption);

        if (!EntMan.TryGetComponent<StellarSocialComponent>(PlayerManager.LocalEntity, out var userEmotes) || _timing.CurTime < userEmotes.SocialCooldown)
            return options; // If we don't have socials, or our stuff is on cooldown, don't offer social options.

        var offerItemOption = new RadialMenuActionOption<EntityUid>(SendItemOfferMessage, ent)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconOfferItem),
            ToolTip = "Offer item",
        };
        options.Add(offerItemOption);

        var social = new List<RadialMenuOptionBase>();
        var socialOption = new RadialMenuNestedLayerOption(social, 75f)
        {
            IconSpecifier = RadialMenuIconSpecifier.With(radialComp.IconSocial),
            ToolTip = "Social",
        };
        options.Add(socialOption);

        foreach (var emote in userEmotes.CoopEmotesAvailable)  // Co-op emote availability is based on the interactee's options, not the target's.
        {
            if (!_proto.TryIndex(emote, out var indexedEmote))
                continue;

            var emoteOption = new RadialMenuActionOption<StellarCoopEmotePrototype>(SendEmoteMessage, indexedEmote)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(indexedEmote.Icon),
                ToolTip = Loc.GetString(indexedEmote.Name),
            };
            social.Add(emoteOption);
        }
        return options;
    }

    private void SendEmoteMessage(StellarCoopEmotePrototype emote)
    {
        var netEnt = EntMan.GetNetEntity(Owner);
        EntMan.RaisePredictiveEvent(new StellarRadialSocialEvent(netEnt, StellarPoPInteractionMethod.Emote, emote.ID));
    }

    private void SendItemOfferMessage(EntityUid target)
    {
        var netEnt = EntMan.GetNetEntity(target);
        EntMan.RaisePredictiveEvent(new StellarRadialSocialEvent(netEnt, StellarPoPInteractionMethod.OfferItem, null));
    }

    private void SendPullMessage(EntityUid target)
    {
        var netEnt = EntMan.GetNetEntity(target);
        EntMan.RaisePredictiveEvent(new StellarRadialEvent(netEnt, StellarPoPInteractionMethod.Pull));
    }

    private void SendStripMessage(EntityUid target)
    {
        var netEnt = EntMan.GetNetEntity(target);
        EntMan.RaisePredictiveEvent(new StellarRadialEvent(netEnt, StellarPoPInteractionMethod.Strip));
    }

    private void SendExamineMessage(EntityUid target)
    {
        var examineSystem = EntMan.System<ExamineSystem>();
        examineSystem.DoExamine(target);
    }
}
