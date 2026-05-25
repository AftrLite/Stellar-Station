// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.Audio;
using Content.Shared.Chat;
using Content.Shared.Dataset;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// The system for the A.B.E and B.E.A. portable science devices.
/// Due to sharing functionalities, they both use this system.
/// </summary>
public abstract class SharedStellarSciencePortablesSystem : EntitySystem
{
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly PullingSystem Pull = default!;
    [Dependency] protected readonly SharedAmbientSoundSystem Ambience = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UiSystem = default!;

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedPopupSystem _popUp = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarStabilizerBeamComponent, HitscanRaycastFiredEvent>(OnHitscanHit);

        // SubscribeLocalEvent<StellarContainmentCapsuleComponent, StellarCapsuleDoAfter>(OnCapsuleDoAfter); // TODO: Move into different system

        SubscribeLocalEvent<StellarSciencePortablesComponent, StellarSciPortableDeployDoAfter>(OnPortablesDeploy);
        SubscribeLocalEvent<StellarSciencePortablesComponent, StellarSciRadialMessage>(OnPortablesMenu);

        SubscribeLocalEvent<StellarSciencePortablesComponent, ExaminedEvent>(OnPortablesExamined);
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
        if (TryComp<StellarSciencePortablesComponent>(abe, out var abeComp))
            PortablesDialogue(abe, abeComp.DialogueStabilized, 1);
    }

    private void HarvestAnomaly(Entity<StellarSciencePortablesComponent> ent)
    {
        var xform = Transform(ent);
        var pos = new EntityCoordinates(ent, new Vector2(0, -1));

        if (xform.GridUid is null || !TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        Entity<StellarAnomalyComponent>? anomaly = null;
        foreach (var target in _map.GetAnchoredEntities(xform.GridUid.Value, grid, pos))
        {
            if (!TryComp<StellarAnomalyComponent>(target, out var anomComp) || anomComp.HarvestDoAfterId != null)
                continue;
            anomaly = (target, anomComp);
        }

        if (anomaly is null)
        {
            _popUp.PopupPredicted(Loc.GetString("bea-popup-no-anom"), ent, ent, PopupType.MediumCaution);
            return;
        }

        if (!anomaly.Value.Comp.Stable)
        {
            _popUp.PopupPredicted(Loc.GetString("bea-popup-anom-unstable"), ent, ent, PopupType.MediumCaution);
            return;
        }

        if (anomaly.Value.Comp.IntegrityPipsValue <= 1)
        {
            _popUp.PopupPredicted(Loc.GetString("bea-popup-anom-fading"), ent, ent, PopupType.MediumCaution);
            return;
        }

        var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.HarvestTime, new StellarBeaHarvestDoAfter(), ent, anomaly)
        {
            Hidden = false, BreakOnMove = true, BreakOnDropItem = false, BreakOnHandChange = false, RequireCanInteract = false,
        };

        Ambience.SetAmbience(ent, true);
        DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
        anomaly.Value.Comp.HarvestDoAfterId = doAfterId;
        ent.Comp.DoAfterId = doAfterId;
        Dirty(ent);
    }

    private void StabilizerBeam(Entity<StellarSciencePortablesComponent> ent, EntityUid user, EntProtoId? beam, EntProtoId? muzzle)
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

    private void PortablesDialogue(EntityUid uid, ProtoId<LocalizedDatasetPrototype>? dialogue, float chance)
    {
        if (!_proto.TryIndex(dialogue, out var proto) || !Random.Prob(chance))
            return;

        var msg = Random.Pick(proto.Values);
        _chat.TrySendInGameICMessage(uid, Loc.GetString(msg), InGameICChatType.Speak, false);
    }

    // private void OnCapsuleDoAfter(Entity<StellarContainmentCapsuleComponent> ent, ref StellarCapsuleDoAfter args)
    // {
    //     if (args.Cancelled || args.Handled || !TryComp<StellarAnomalyComponent>(args.Target, out var anomComp))
    //         return;
    //
    //     if (anomComp.IntegrityPipsValue is not { } pips || pips < 1)
    //         return;
    //
    //     ent.Comp.Full = true;
    //     ent.Comp.StoredEnergy = pips;
    //     anomComp.IntegrityPipsValue = 0;
    //     Appearance.SetData(ent, StellarCapsuleVisuals.Visuals, true);
    //
    //     var ev = new StellarAnomalyDecrementEvent();
    //     RaiseLocalEvent(args.Target.Value, ref ev);
    //     Dirty(ent);
    // }

    private void OnPortablesDeploy(Entity<StellarSciencePortablesComponent> ent, ref StellarSciPortableDeployDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!Transform(ent).Anchored)
            Appearance.SetData(ent, StellarPortablesVisuals.Visuals, StellarPortablesState.Undeployed);
        else
            Appearance.SetData(ent, StellarPortablesVisuals.Visuals, StellarPortablesState.Deployed);

        var dialogue = Transform(ent).Anchored ? ent.Comp.DialogueDeployed : ent.Comp.DialogueUndeployed;
        ent.Comp.DoAfterId = null;

        PortablesDialogue(ent, dialogue, ent.Comp.DialogueChance);
        Dirty(ent);
    }

    protected virtual void OnPortablesMenu(Entity<StellarSciencePortablesComponent> ent, ref StellarSciRadialMessage args)
    {
        switch (args.Method)
        {
            case SciPortableMenuMethod.Deploy:
                {
                    if (Transform(ent).Anchored)
                        Audio.PlayPredicted(ent.Comp.SoundUndeploy, ent, args.Actor);
                    else
                        Audio.PlayPredicted(ent.Comp.SoundDeploy, ent, args.Actor);
                    var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.TransformTime, new StellarSciPortableDeployDoAfter(), ent, ent)
                    {
                        Hidden = false, BreakOnMove = false, BreakOnWeightlessMove = false, BreakOnDropItem = false, BreakOnHandChange = false, RequireCanInteract = false,
                    };
                    UiSystem.CloseUi(ent.Owner, StellarPortablesRadialKey.Key);
                    DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
                    ent.Comp.DoAfterId = doAfterId;
                    Dirty(ent);
                }
                return;
            case SciPortableMenuMethod.Pull:
                Pull.TogglePull(ent.Owner, args.Actor);
                return;
            case SciPortableMenuMethod.Rotate:
                TransformSystem.SetLocalRotation(ent.Owner, Transform(ent).LocalRotation + Angle.FromDegrees(90));
                return;
            case SciPortableMenuMethod.ShootBeam:
                StabilizerBeam(ent, args.Actor, args.Beam, args.Muzzle);
                return;
            case SciPortableMenuMethod.Harvest:
                HarvestAnomaly(ent);
                return;
        }
    }

    private void OnPortablesExamined(Entity<StellarSciencePortablesComponent> ent, ref ExaminedEvent args)
    {
        switch (ent.Comp.PortablesType)
        {
            case StellarPortablesType.Abe:
                args.PushMarkup(Loc.GetString("abe-examine"));
                break;
            case StellarPortablesType.Bea:
                args.PushMarkup(Loc.GetString("bea-examine"));
                break;
        }
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
public sealed partial class StellarBeaHarvestDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StellarCapsuleDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StellarSciPortableDeployDoAfter : SimpleDoAfterEvent;

// [Serializable, NetSerializable]
// public sealed partial class StellarAnchorDisableDoAfter : SimpleDoAfterEvent;

// [Serializable, NetSerializable]
// public sealed partial class StellarAnchorAutoEnableDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class StellarSciRadialMessage(SciPortableMenuMethod method, EntProtoId? beam = null, EntProtoId? muzzle = null) : BoundUserInterfaceMessage
{
    public SciPortableMenuMethod Method = method;

    public EntProtoId? Beam = beam;

    public EntProtoId? Muzzle = muzzle;
}

[Serializable, NetSerializable]
public enum SciPortableMenuMethod : byte
{
    Deploy,
    Pull,
    Rotate,
    ShootBeam,
    Harvest,
}
