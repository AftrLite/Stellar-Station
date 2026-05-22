// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science;


/// <summary>
/// The system for the handful of technology that interacts with anomalies.
/// The Containment Capsule's anomaly-interaction event can be found in the SharedStellarScienceAnomalySystem.
/// </summary>
public abstract class SharedStellarScienceAbeSystem : EntitySystem
{
    [Dependency] protected readonly PullingSystem Pull = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UiSystem = default!;

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarStabilizerBeamComponent, HitscanRaycastFiredEvent>(OnHitscanHit);

        SubscribeLocalEvent<StellarContainmentCapsuleComponent, StellarCapsuleDoAfter>(OnCapsuleDoAfter);

        SubscribeLocalEvent<StellarAbeComponent, StellarAbeDeployDoAfter>(OnAbeDeploy);
        SubscribeLocalEvent<StellarAbeComponent, StellarAbeRadialMessage>(OnAbeMenu);

        SubscribeLocalEvent<StellarAbeComponent, ExaminedEvent>(OnAbeExamined);
    }

    private void OnHitscanHit(Entity<StellarStabilizerBeamComponent> ent, ref HitscanRaycastFiredEvent args)
    {
        if (args.Data.HitEntity == null)
            return;

        var hitEnt = args.Data.HitEntity;
        var abe = args.Data.Gun;
        _stun.TryAddParalyzeDuration(hitEnt.Value, TimeSpan.FromSeconds(1.5f));
        _stun.TrySeeingStars(hitEnt.Value);

        if (!TryComp<StellarAnomalyComponent>(hitEnt, out var anomComp) || anomComp.Stable)
            return;

        if (anomComp.EnteredCode.Length < anomComp.CodeLength)
            anomComp.EnteredCode += ent.Comp.Digit.ToString();

        var codeCache = anomComp.EnteredCode;
        for (int i = 0; i < anomComp.EnteredCode.Length; i++)
        {
            if (!anomComp.EnteredCode[i].Equals(anomComp.AnomalyCode[i]))
            {
                anomComp.EnteredCode = string.Empty;
            }
        }

        Dirty(ent);
        RaiseNetworkEvent(new StellarAnomalyReactionVisualsEvent(GetNetEntity(hitEnt.Value), GetNetEntity(args.Data.Gun), codeCache));
        if (!anomComp.EnteredCode.Equals(anomComp.AnomalyCode))
            return;

        var ev = new StellarAnomalyStabilizeEvent();
        RaiseLocalEvent(hitEnt.Value, ref ev);
        if (TryComp<StellarAbeComponent>(abe, out var abeComp) && abeComp.DialogueStabilized != null && _proto.TryIndex(abeComp.DialogueStabilized, out var proto))
        {
            var msg = _random.Pick(proto.Values);
            _chat.TrySendInGameICMessage(abe, Loc.GetString(msg), InGameICChatType.Speak, false);
        }
    }

    private void StabilizerBeam(Entity<StellarAbeComponent> ent, EntityUid user, EntProtoId? beam, EntProtoId? muzzle)
    {
        if (beam is null || muzzle is null)
            return;

        var mapDirection = TransformSystem.ToMapCoordinates(new EntityCoordinates(ent, new Vector2(0, -1))).Position - TransformSystem.GetMapCoordinates(ent).Position;
        var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.BeamChargeTime, new StellarAbeBeamDoAfter(mapDirection, beam.Value, muzzle.Value), ent, ent)
        {
            Hidden = false, BreakOnMove = true, BreakOnDropItem = false, BreakOnHandChange = false, RequireCanInteract = false,
        };

        Audio.PlayPredicted(ent.Comp.SoundCharge, ent, user);
        DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
        ent.Comp.DoAfterId = doAfterId;
        Dirty(ent);
    }

    private void OnCapsuleDoAfter(Entity<StellarContainmentCapsuleComponent> ent, ref StellarCapsuleDoAfter args)
    {
        if (args.Cancelled || args.Handled || !TryComp<StellarAnomalyComponent>(args.Target, out var anomComp))
            return;

        if (anomComp.IntegrityPipsValue is not { } pips || pips < 1)
            return;

        ent.Comp.Full = true;
        ent.Comp.StoredEnergy = pips;
        anomComp.IntegrityPipsValue = 0;
        Appearance.SetData(ent, StellarCapsuleVisuals.Visuals, true);

        var ev = new StellarAnomalyDecrementEvent();
        RaiseLocalEvent(args.Target.Value, ref ev);
        Dirty(ent);
    }

    private void OnAbeDeploy(Entity<StellarAbeComponent> ent, ref StellarAbeDeployDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!Transform(ent).Anchored)
            Appearance.SetData(ent, StellarAbeVisuals.Visuals, StellarAbeState.Undeployed);
        else
            Appearance.SetData(ent, StellarAbeVisuals.Visuals, StellarAbeState.Deployed);

        var dialogue = Transform(ent).Anchored ? ent.Comp.DialogueDeployed : ent.Comp.DialogueUndeployed;
        if (_random.Prob(ent.Comp.DialogueChance) && _proto.TryIndex(dialogue, out var proto))
        {
            var msg = _random.Pick(proto.Values);
            _chat.TrySendInGameICMessage(ent, Loc.GetString(msg), InGameICChatType.Speak, true);
        }

        ent.Comp.DoAfterId = null;
        Dirty(ent);
    }

    protected virtual void OnAbeMenu(Entity<StellarAbeComponent> ent, ref StellarAbeRadialMessage args)
    {
        switch (args.Method)
        {
            case AbeMenuMethod.Deploy:
                {
                    if (Transform(ent).Anchored)
                        Audio.PlayPredicted(ent.Comp.SoundUndeploy, ent, args.Actor);
                    else
                        Audio.PlayPredicted(ent.Comp.SoundDeploy, ent, args.Actor);
                    var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.TransformTime, new StellarAbeDeployDoAfter(), ent, ent)
                    {
                        Hidden = false, BreakOnMove = false, BreakOnWeightlessMove = false, BreakOnDropItem = false, BreakOnHandChange = false, RequireCanInteract = false,
                    };
                    UiSystem.CloseUi(ent.Owner, StellarAbeRadialKey.Key);
                    DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
                    ent.Comp.DoAfterId = doAfterId;
                    Dirty(ent);
                }
                return;
            case AbeMenuMethod.Pull:
                Pull.TogglePull(ent.Owner, args.Actor);
                return;
            case AbeMenuMethod.Rotate:
                TransformSystem.SetLocalRotation(ent.Owner, Transform(ent).LocalRotation + Angle.FromDegrees(90));
                return;
            case AbeMenuMethod.ShootBeam:
                StabilizerBeam(ent, args.Actor, args.Beam, args.Muzzle);
                return;
        }
    }

    private void OnAbeExamined(Entity<StellarAbeComponent> ent, ref ExaminedEvent args)
    {
        // Examine abe
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarAbeBeamDoAfter : DoAfterEvent
{
    [DataField] public Vector2 MapDirection;

    [DataField] public EntProtoId Beam;

    [DataField] public EntProtoId Muzzle;

    public StellarAbeBeamDoAfter(Vector2 mapDirection, EntProtoId beam, EntProtoId muzzle)
    {
        MapDirection = mapDirection;
        Beam = beam;
        Muzzle = muzzle;
    }

    public override DoAfterEvent Clone() => this;
}
[Serializable, NetSerializable]
public sealed partial class StellarCapsuleDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StellarAbeDeployDoAfter : SimpleDoAfterEvent;

// [Serializable, NetSerializable]
// public sealed partial class StellarAnchorDisableDoAfter : SimpleDoAfterEvent;

// [Serializable, NetSerializable]
// public sealed partial class StellarAnchorAutoEnableDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class StellarAbeRadialMessage(AbeMenuMethod method, EntProtoId? beam = null, EntProtoId? muzzle = null) : BoundUserInterfaceMessage
{
    public AbeMenuMethod Method = method;

    public EntProtoId? Beam = beam;

    public EntProtoId? Muzzle = muzzle;
}

[Serializable, NetSerializable]
public enum AbeMenuMethod : byte
{
    Deploy,
    Pull,
    Rotate,
    ShootBeam,
}
