// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Chat;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Stellar.Shared.CCVars;
using Content.Stellar.Shared.Interaction;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Social;

/// <summary>
/// The stellar social system.
/// Works in tandem with the StellarPoPRadialBoundUserInterface to make Social Emotes and Item Handovers/handoffs happen.
/// </summary>
public abstract class SharedStellarSocialSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem PopUp = default!;

    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;

    private TimeSpan _socialCooldown;
    private float _socialRange;

    public override void Initialize()
    {
        SubscribeLocalEvent<StellarOfferedItemComponent, HandDeselectedEvent>(OnItemDeselected);
        SubscribeLocalEvent<StellarSocialComponent, MoveInputEvent>(OnMove);
        SubscribeLocalEvent<StellarSocialComponent, InteractHandEvent>(OnInteractHand);

        SubscribeLocalEvent<StellarSocialComponent, EmoteEvent>(OnEmote);
        SubscribeLocalEvent<StellarSocialComponent, BeforeEmoteEvent>(OnBeforeEmote);
        SubscribeLocalEvent<StellarSocialComponent, InventoryRelayedEvent<BeforeEmoteEvent>>(OnRelayedEmoteEvent);

        SubscribeAllEvent<StellarRadialSocialEvent>(OnInteractionRadialSocial);

        _config.OnValueChanged(STCCVars.SocialCooldownTime, (c => _socialCooldown = TimeSpan.FromSeconds(c)), true);
        _config.OnValueChanged(STCCVars.SocialInteractionRange, (r => _socialRange = r), true);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var coreQuery = EntityQueryEnumerator<StellarSocialComponent>();
        while (coreQuery.MoveNext(out var ent, out var comp))
        {
            if (comp.ResponseTimeout != null && _timing.CurTime > comp.ResponseTimeout)
            {
                ExpireSocialRequests((ent, comp));
            }
        }
    }

    #region Emote cooldowns
    private void OnRelayedEmoteEvent(Entity<StellarSocialComponent> entity, ref InventoryRelayedEvent<BeforeEmoteEvent> args)
    {
        OnBeforeEmote(entity, ref args.Args);
    }

    private void OnBeforeEmote(Entity<StellarSocialComponent> ent, ref BeforeEmoteEvent args)
    {
        if (_timing.CurTime < ent.Comp.SocialCooldown)
        {
            args.Cancel();
            args.Blocker = ent;
        }
    }

    private void OnEmote(Entity<StellarSocialComponent> ent, ref EmoteEvent args)
    {
        ent.Comp.SocialCooldown = _timing.CurTime + _socialCooldown;
        Dirty(ent);
    }
    #endregion

    protected virtual void OnItemDeselected(Entity<StellarOfferedItemComponent> ent, ref HandDeselectedEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        RemComp<StellarOfferedItemComponent>(ent);
        ExpireSocialRequests(args.User);
    }

    protected virtual void OnMove(Entity<StellarSocialComponent> ent, ref MoveInputEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        ExpireSocialRequests((ent.Owner, ent.Comp));
    }

    private void OnInteractionRadialSocial(StellarRadialSocialEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } ent || !TryGetEntity(msg.Target, out var target) || !TryComp<StellarSocialComponent>(ent, out var socialComp))
            return;

        if (msg.Method == StellarPoPInteractionMethod.OfferItem)
        {
            if (!_hands.TryGetActiveItem(ent, out var activeItem) || TerminatingOrDeleted(activeItem))
            {
                PopUp.PopupCursor(Loc.GetString("stellar-social-item-held-error", ("user", ent), ("target", target.Value)));
                return;
            }

            socialComp.OfferedItem = activeItem;
            EnsureComp<StellarOfferedItemComponent>(activeItem.Value);
            PopUp.PopupPredicted(Loc.GetString("stellar-social-item-offer", ("user", ent), ("target", target.Value), ("item", Identity.Name(activeItem.Value, EntityManager))), ent, ent);
        }

        if (msg.Method == StellarPoPInteractionMethod.Emote && _proto.TryIndex(msg.Emote, out var proto) && proto.Icon != null)
        {
            socialComp.RequestedEmote = msg.Emote;
            PopUp.PopupPredicted(Loc.GetString(proto.PopUpRequest, ("user", ent), ("target", target.Value)), ent, ent);
        }

        socialComp.Target = target.Value;
        socialComp.ResponseTimeout = _timing.CurTime + socialComp.TimeoutTime;
        socialComp.SocialCooldown = _timing.CurTime + _socialCooldown;
        RaiseNetworkEvent(new StellarSocialRequestEvent(GetNetEntity(target.Value), GetNetEntity(ent)));
        Dirty(ent, socialComp);
    }

    private void OnInteractHand(Entity<StellarSocialComponent> ent, ref InteractHandEvent args)
    {
        if (ent.Comp.Target == null || ent.Comp.Target != args.User || !TryComp<StellarSocialComponent>(args.User, out var socialComp))
            return;

        var interactor = args.User;
        var requestee = args.Target;

        if (!_interaction.InRangeUnobstructed(interactor, requestee, _socialRange))
        {
            PopUp.PopupClient(Loc.GetString("stellar-social-distance-error", ("target", ent)), interactor, interactor);
            args.Handled = true;
            return;
        }

        socialComp.SocialCooldown = _timing.CurTime + _socialCooldown;
        ent.Comp.SocialCooldown = _timing.CurTime + _socialCooldown;
        ent.Comp.Target = null;
        Dirty(interactor, socialComp);
        Dirty(ent);

        if (ent.Comp.OfferedItem != null)
        {
            if (!_hands.TryGetActiveItem(ent.Owner, out var activeItem) || activeItem != ent.Comp.OfferedItem)
                return;
            if (_hands.GetActiveHand(ent.Owner) is not { } activeHand)
                return;

            args.Handled = true;
            RaiseNetworkEvent(new StellarTransferItemVisualsEvent(GetNetEntity(interactor), GetNetEntity(requestee), GetNetEntity(activeItem.Value), MetaData(activeItem.Value).EntityName));
            TransferItem(interactor, requestee, activeItem.Value, activeHand);
            return;
        }

        args.Handled = true;
        RaiseNetworkEvent(new StellarCoopEmoteVisualsEvent(GetNetEntity(interactor), GetNetEntity(requestee)));
    }

    /// <remarks>
    /// Social requests expire when:
    /// - The Player moves, drops an item (when offering an item), or swaps hands (when offering an item).
    /// - The social request times out without being responded to by the target.
    /// </remarks>
    private void ExpireSocialRequests(Entity<StellarSocialComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.Target = null;
        ent.Comp.OfferedItem = null;
        ent.Comp.RequestedEmote = null;
        ent.Comp.ResponseTimeout = null;
        Dirty(ent);
    }

    private void TransferItem(Entity<HandsComponent?> user, Entity<HandsComponent?> target, EntityUid item, string handName)
    {
        if (!Resolve(user, ref user.Comp) || !Resolve(target, ref target.Comp) || !Resolve(target, ref target.Comp) || !target.Comp.CanBeStripped)
            return;

        if (!_hands.TryGetHand(target, handName, out _) || !_hands.TryGetHeldItem(target, handName, out var heldEntity))
            return;

        if (HasComp<VirtualItemComponent>(heldEntity) || heldEntity != item || !_hands.CanDropHeld(target, handName, false))
            return;

        _hands.TryDrop(target, item, checkActionBlocker: false);
        _hands.PickupOrDrop(user, item, false, true, handsComp: user.Comp);
    }
}

#region Events
[Serializable, NetSerializable]
public sealed partial class StellarTransferItemVisualsEvent : EntityEventArgs
{
    public NetEntity Target;

    public NetEntity Requestee;

    public NetEntity Item;

    public string ItemName;

    public StellarTransferItemVisualsEvent(NetEntity target, NetEntity requestee, NetEntity item, string itemName)
    {
        Target = target;
        Requestee = requestee;
        Item = item;
        ItemName = itemName;
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarCoopEmoteVisualsEvent : EntityEventArgs
{
    public NetEntity Target;

    public NetEntity Requestee;

    public StellarCoopEmoteVisualsEvent(NetEntity target, NetEntity requestee)
    {
        Target = target;
        Requestee = requestee;
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarSocialRequestEvent : EntityEventArgs
{
    public NetEntity Target;

    public NetEntity Requestee;

    public StellarSocialRequestEvent(NetEntity target, NetEntity requestee)
    {
        Target = target;
        Requestee = requestee;
    }
}
#endregion
