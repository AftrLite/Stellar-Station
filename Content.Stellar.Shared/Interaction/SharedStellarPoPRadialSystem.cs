// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.ActionBlocker;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Strip;
using Content.Shared.Strip.Components;
using Content.Stellar.Shared.Social;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Interaction;

/// <summary>
/// Handles the Stellar Player-on-Player interaction radial menu system's basic interactions,
/// also including the Pulling, Inspecting (Stripping), and Examining functionality.
/// </summary>
public sealed class SharedStellarPoPRadialSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedStrippableSystem _strippable = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarPoPRadialComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<StellarPoPRadialComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<StellarPoPRadialComponent, StellarRadialInspectDoAfter>(OnInspectDoAfter);

        SubscribeAllEvent<StellarRadialEvent>(OnInteractionRadial);
    }

    private void OnActivateInWorld(Entity<StellarPoPRadialComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.User == args.Target)
            return;

        PendingSocials(args.Target, args.User, ent.Owner);
    }

    private void OnInteractHand(Entity<StellarPoPRadialComponent> ent, ref InteractHandEvent args)
    {
        if (args.User == args.Target || args.Handled)
            return;

        PendingSocials(args.Target, args.User, ent.Owner);
    }

    private void PendingSocials(EntityUid target, EntityUid user, EntityUid uid)
    {
        if (TryComp<StellarSocialComponent>(target, out var socialComp) && socialComp.Target == user)
            return;

        _ui.OpenUi(uid, StellarPoPRadialKey.Key, user, true);
    }

    private void OnInteractionRadial(StellarRadialEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } ent || !TryGetEntity(msg.Target, out var target))
            return;

        if (!_interaction.IsAccessible(ent, target.Value) || !_actionBlocker.CanInteract(ent, target.Value))
            return;

        switch (msg.Method)
        {
            case StellarPoPInteractionMethod.Pull:
                _pulling.TogglePull(target.Value, ent);
                break;
            case StellarPoPInteractionMethod.Strip:
                var doArgs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(1), new StellarRadialInspectDoAfter(), ent, target.Value)
                {
                    Hidden = true,
                    BreakOnMove = true,
                    BreakOnWeightlessMove = true,
                };
                _doAfter.TryStartDoAfter(doArgs);
                break;
        }
    }

    private void OnInspectDoAfter(Entity<StellarPoPRadialComponent> ent, ref StellarRadialInspectDoAfter args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target == null)
            return;

        if (TryComp<StrippableComponent>(args.Args.Target, out var strippable))
        {
            _strippable.TryOpenStrippingUi(ent, (args.Args.Target.Value, strippable));
        }
    }
}

#region Events
[Serializable, NetSerializable]
public sealed class StellarRadialEvent : EntityEventArgs
{
    public NetEntity Target;
    public StellarPoPInteractionMethod Method;

    public StellarRadialEvent(NetEntity target, StellarPoPInteractionMethod method)
    {
        Target = target;
        Method = method;
    }
}

/// <summary>
/// What's the difference between this one and the StellarRadialEvent?
/// This one gets handled by the StellarSocialSystem, not by the StellarInteractionRadialSystem.
/// SocialSystem is stuff like Emotes, Item handovers, et cetera, all necessitating a "Social Request". InteractionRadialSystem is for direct gamesystem interactions.
/// </summary>
[Serializable, NetSerializable]
public sealed class StellarRadialSocialEvent : EntityEventArgs
{
    public NetEntity Target;
    public StellarPoPInteractionMethod? Method;
    public ProtoId<StellarCoopEmotePrototype>? Emote;

    public StellarRadialSocialEvent(NetEntity target, StellarPoPInteractionMethod? method, ProtoId<StellarCoopEmotePrototype>? emote)
    {
        Target = target;
        Emote = emote;
        Method = method;
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarRadialInspectDoAfter : SimpleDoAfterEvent;
#endregion
