// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Linq;
using System.Numerics;
using Content.Shared.ActionBlocker;
using Content.Shared.Climbing.Systems;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Examine;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Stellar.Shared._ES.Core.Timer;
using Content.Stellar.Shared._ES.Core.Timer.Components;
using Content.Stellar.Shared.Transition;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.RecyclerChute;

public abstract class SharedStellarRecyclerChuteSystem : EntitySystem
{
    [Dependency] protected readonly ESEntityTimerSystem Timers = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly ISharedPlayerManager Player = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;
    [Dependency] protected readonly SharedPopupSystem PopUp = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;

    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedStellarTransitionSystem _transition = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    private static readonly EntProtoId StunId = "StatusEffectStunned";

    protected HashSet<Entity<StellarChuteDestinationComponent, TransformComponent>> DestinationSet = new();
    protected HashSet<Entity<StellarChuteTravelMarkerComponent, TransformComponent>> TravelSet = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarRecyclerChuteComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, ActivateInWorldEvent>(OnActivateInWorld);

        SubscribeLocalEvent<StellarRecyclerChuteComponent, StellarChuteRadialMessage>(OnRadialMenu);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<StellarRecyclerChuteComponent, AfterInteractUsingEvent>(OnAfterInteractUsing);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, EntInsertedIntoContainerMessage>(OnEntityInserted);

        SubscribeLocalEvent<StellarRecyclerChuteComponent, StellarInsertChuteDoAfterEvent>(OnInsertDoAfter);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, StellarChargeChuteDoAfterEvent>(OnChargeDoAfter);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, StellarChuteChargedEvent>(OnChargeComplete);

        SubscribeLocalEvent<StellarRecyclerChuteComponent, DragDropTargetEvent>(OnDragDropOn);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, CanDropTargetEvent>(OnCanDragDropOn);
        SubscribeLocalEvent<StellarRecyclerChuteComponent, ContainerRelayMovementEntityEvent>(OnMovement);

        SubscribeLocalEvent<StellarChuteTravellingComponent, AttemptMobCollideEvent>(OnMobCollide);
    }

    private void OnActivateInWorld(Entity<StellarRecyclerChuteComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.Handled || ent.Comp.State == ChuteState.Cooldown)
            return;

        if (Container.ContainsEntity(ent, args.User))
            return;

        _ui.OpenUi(ent.Owner, StellarChuteRadialKey.Key, args.User);
    }

    private void OnInteractHand(Entity<StellarRecyclerChuteComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || ent.Comp.State == ChuteState.Cooldown)
            return;

        if (Container.ContainsEntity(ent, args.User))
        {
            if (Timing.IsFirstTimePredicted)
                PopUp.PopupClient(Loc.GetString("stellar-chute-popup-inside"), args.User, args.User);
            return;
        }

        _ui.OpenUi(ent.Owner, StellarChuteRadialKey.Key, args.User);
    }

    private void OnRadialMenu(Entity<StellarRecyclerChuteComponent> ent, ref StellarChuteRadialMessage args)
    {
        switch (args.Method)
        {
            case ChuteMenuMethod.Activate:
                if (ent.Comp.State == ChuteState.Idle && Timing.IsFirstTimePredicted && !Timing.ApplyingState)
                {
                    var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.ChargeTime, new StellarChargeChuteDoAfterEvent(), ent, ent)
                    {
                        BreakOnDamage = true,
                        NeedHand = false,
                    };
                    DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
                    var streamEnt = Audio.PlayPredicted(ent.Comp.SoundCharge, ent, args.Actor);
                    ent.Comp.ChargeAudioStream = streamEnt?.Entity;
                    ent.Comp.State = ChuteState.Charging;
                    ent.Comp.DoAfterId = doAfterId;

                    Dirty(ent);
                    Appearance.SetData(ent, ChuteVisuals.Base, ent.Comp.State);
                }
                break;
            case ChuteMenuMethod.Insert:
                TryInsert((ent.Owner, ent.Comp), args.Actor, args.Actor);
                break;
            case ChuteMenuMethod.Eject:
                foreach (var entity in Container.GetContainer(ent, ent.Comp.ContainerId).ContainedEntities.ToArray())
                {
                    Remove(ent, entity);
                    Physics.ApplyLinearImpulse(entity, new Vector2(Random.NextFloat(-5, +5), Random.NextFloat(-5, +5)) * 30);
                    Physics.ApplyAngularImpulse(entity, Random.NextFloat(-12, +12));
                }
                break;
        }
    }

    private void OnExamine(Entity<StellarRecyclerChuteComponent> ent, ref ExaminedEvent args)
    {
        switch (ent.Comp.State)
        {
            case ChuteState.Idle:
                args.PushMarkup(Loc.GetString("stellar-chute-state-idle"));
                break;
            case ChuteState.Charging:
                args.PushMarkup(Loc.GetString("stellar-chute-state-charging"));
                break;
            case ChuteState.Cooldown:
                args.PushMarkup(Loc.GetString("stellar-chute-state-cooldown"));
                break;
        }
    }

    private void OnInsertAttempt(Entity<StellarRecyclerChuteComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (ent.Comp.State == ChuteState.Cooldown)
        {
            args.Cancel();
        }
    }

    private void OnEntityInserted(Entity<StellarRecyclerChuteComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (Timing.IsFirstTimePredicted)
            Audio.PlayEntity(ent.Comp.SoundInsert, ent, args.Entity);

        if (ent.Comp.AutoActivateTimer == null)
        {
            ent.Comp.AutoActivateTimer = Timing.CurTime + ent.Comp.AutoActivateTime;
        }
        Dirty(ent);
    }

    private void OnAfterInteractUsing(Entity<StellarRecyclerChuteComponent> ent, ref AfterInteractUsingEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        if (!_handsSystem.TryDropIntoContainer(args.User, args.Used, Container.GetContainer(ent, ent.Comp.ContainerId)))
            return;

        args.Handled = true;
    }

    private void OnMovement(Entity<StellarRecyclerChuteComponent> ent, ref ContainerRelayMovementEntityEvent args)
    {
        if (!_actionBlocker.CanMove(args.Entity) || !TryComp(args.Entity, out HandsComponent? hands) || hands.Count == 0)
            return;

        Remove(ent, args.Entity);
    }

    private void OnCanDragDropOn(Entity<StellarRecyclerChuteComponent> ent, ref CanDropTargetEvent args)
    {
        if (args.Handled || ent.Comp.State == ChuteState.Cooldown)
            return;

        args.CanDrop = Container.CanInsert(args.Dragged, Container.GetContainer(ent, ent.Comp.ContainerId));
        args.Handled = true;
    }

    private void OnDragDropOn(Entity<StellarRecyclerChuteComponent> ent, ref DragDropTargetEvent args)
    {
        args.Handled = TryInsert((ent.Owner, ent.Comp), args.Dragged, args.User);
    }

    private void OnInsertDoAfter(Entity<StellarRecyclerChuteComponent> ent, ref StellarInsertChuteDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        args.Handled = Container.Insert(args.Target.Value, Container.GetContainer(ent, ent.Comp.ContainerId));
    }

    private void OnChargeDoAfter(Entity<StellarRecyclerChuteComponent> ent, ref StellarChargeChuteDoAfterEvent args)
    {
        if (args.Handled)
            return;

        var contents = Container.GetContainer(ent, ent.Comp.ContainerId);
        ent.Comp.DoAfterId = null;
        ent.Comp.AutoActivateTimer = null;
        var travelTime = ent.Comp.TravelTimeMin;
        ent.Comp.ChargeAudioStream = Audio.Stop(ent.Comp.ChargeAudioStream);
        ent.Comp.State = args.Cancelled ? ChuteState.Idle : ChuteState.Cooldown;

        if (args.Cancelled && contents.ContainedEntities.Count > 0)
        {
            ent.Comp.AutoActivateTimer = Timing.CurTime + ent.Comp.AutoActivateTime;
            Appearance.SetData(ent, ChuteVisuals.Base, ent.Comp.State);
            args.Handled = true;
            return;
        }

        ent.Comp.CooldownTimer = Timing.CurTime + ent.Comp.CooldownTime;
        Appearance.SetData(ent, ChuteVisuals.Base, ent.Comp.State);
        DirtyField(ent.Owner, ent.Comp, nameof(StellarRecyclerChuteComponent.State));

        if (ent.Comp.State == ChuteState.Cooldown)
        {
            Audio.PlayPredicted(ent.Comp.SoundFlush, ent, ent);
            Timers.SpawnTimer(ent, TimeSpan.FromSeconds(0.5), new StellarChuteChargedEvent());

            foreach (var entity in Container.GetContainer(ent, ent.Comp.ContainerId).ContainedEntities)
            {
                _transition.DoTransition(entity);
                _transition.DoTransition(entity, TimeSpan.FromSeconds(0.5));
                _transition.DoTransition(entity, travelTime - TimeSpan.FromSeconds(0.5));
                _transition.DoTransition(entity, travelTime + TimeSpan.FromSeconds(0.5));
            }
        }
        args.Handled = true;
    }

    private void OnChargeComplete(Entity<StellarRecyclerChuteComponent> ent, ref StellarChuteChargedEvent args)
    {
        var travelTime = ent.Comp.TravelTimeMin;

        HashSet<EntityUid> chuteQueue = new();
        foreach (var entity in Container.GetContainer(ent, ent.Comp.ContainerId).ContainedEntities)
        {
            chuteQueue.Add(entity);
        }

        var travelMarker = TransformSystem.GetMapCoordinates(TravelSet.First()); // There should only be one entry here, anyway.
        foreach (var entity in chuteQueue)
        {
            EnsureComp<StellarChuteTravellingComponent>(entity, out var travelComp);
            Container.Remove(entity, Container.GetContainer(ent, ent.Comp.ContainerId));
            TransformSystem.SetMapCoordinates(entity, travelMarker);
            _status.TryAddStatusEffectDuration(entity, StunId, travelTime + TimeSpan.FromSeconds(1f)); // We use StatusEffects for a Stun here rather than the _stunsystem in order to force a stun but not force the player into dropping the item in their hands.
            travelComp.ArrivalTime = Timing.CurTime + travelTime;

            RaiseNetworkEvent(new StellarChuteAnimEvent(GetNetEntity(entity), travelTime));
        }
    }

    private void OnMobCollide(Entity<StellarChuteTravellingComponent> ent, ref AttemptMobCollideEvent args)
    {
        args.Cancelled = true;
    }

    private bool TryInsert(Entity<StellarRecyclerChuteComponent> ent, EntityUid victim, EntityUid user)
    {
        if (!HasComp<HandsComponent>(user) && victim != user) // Mobs like mouse can Jump inside even with no hands
            return false;

        var insertingSelf = user == victim;
        var delay = insertingSelf ? ent.Comp.SelfEnterDoAfter : ent.Comp.OtherEnterDoAfter;

        if (!insertingSelf)
            PopUp.PopupEntity(Loc.GetString("disposal-unit-being-inserted", ("user", Identity.Entity(victim, EntityManager))), victim, victim, PopupType.Large);

        var doArgs = new DoAfterArgs(EntityManager, user, delay, new StellarInsertChuteDoAfterEvent(), ent, target: victim, used: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = false,
        };

        DoAfter.TryStartDoAfter(doArgs);
        return true;
    }

    private void Remove(Entity<StellarRecyclerChuteComponent> ent, EntityUid toRemove)
    {
        if (!Container.Remove(toRemove, Container.GetContainer(ent, ent.Comp.ContainerId)))
            return;

        if (Container.GetContainer(ent, ent.Comp.ContainerId).ContainedEntities.Count == 0)
        {
            DoAfter.Cancel(ent.Comp.DoAfterId);
            ent.Comp.State = ChuteState.Idle;
            ent.Comp.AutoActivateTimer = null;
            ent.Comp.DoAfterId = null;
            Dirty(ent);
        }

        _climb.Climb(toRemove, toRemove, ent, silent: true);
        Appearance.SetData(ent, ChuteVisuals.Base, ent.Comp.State);
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarInsertChuteDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StellarChargeChuteDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class StellarChuteRadialMessage(NetEntity target, ChuteMenuMethod method) : BoundUserInterfaceMessage
{
    public NetEntity Target = target;

    public ChuteMenuMethod Method = method;
}

[Serializable, NetSerializable]
public sealed partial class StellarChuteAnimEvent(NetEntity target, TimeSpan travelTime, bool? remove = false) : EntityEventArgs
{
    public NetEntity Target = target;

    public TimeSpan TravelTime = travelTime;

    public bool? Remove = remove;
}

[NetSerializable, Serializable]
public sealed partial class StellarChuteChargedEvent : ESEntityTimerEvent;

