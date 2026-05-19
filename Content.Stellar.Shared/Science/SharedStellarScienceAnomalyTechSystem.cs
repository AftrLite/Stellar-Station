// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Hitscan.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science;

public abstract class SharedStellarScienceAnomalyTechSystem : EntitySystem
{
    [Dependency] protected readonly PullingSystem Pull = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyAbeComponent, StellarAbeDeployDoAfter>(OnAbeDeploy);
        SubscribeLocalEvent<StellarAnomalyAbeComponent, StellarAbeRadialMessage>(OnAbeMenu);

        SubscribeLocalEvent<StellarRealityAnchorComponent, ActivateInWorldEvent>(OnAnchorActivateInWorld);
        SubscribeLocalEvent<StellarRealityAnchorComponent, InteractHandEvent>(OnAnchorInteractHand);

        SubscribeLocalEvent<StellarAnomalyAbeComponent, ExaminedEvent>(OnApeExamined);
        SubscribeLocalEvent<StellarAnomalyComponent, ExaminedEvent>(OnAnomalyExamined);
        SubscribeLocalEvent<StellarRealityAnchorComponent, ExaminedEvent>(OnAnchorExamined);
        SubscribeLocalEvent<StellarContainmentCapsuleComponent, ExaminedEvent>(OnCapsuleExamined);
    }

    private void StabilizerBeam(Entity<StellarAnomalyAbeComponent> ent, EntProtoId? beamType, EntityUid user)
    {
        var offset = new EntityCoordinates(ent, new Vector2(0, -1));
        var mapDirection = TransformSystem.ToMapCoordinates(offset).Position - TransformSystem.GetMapCoordinates(ent).Position;

        var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.BeamChargeTime, new StellarAbeBeamDoAfter(mapDirection, beamType), ent, ent)
        {
            Hidden = false, BreakOnMove = true, BreakOnDropItem = false, BreakOnHandChange = false, RequireCanInteract = false,
        };

        Audio.PlayPredicted(ent.Comp.SoundCharge, ent, user);
        DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
        ent.Comp.DoAfterId = doAfterId;
        Dirty(ent);
    }

    private void OnAbeDeploy(Entity<StellarAnomalyAbeComponent> ent, ref StellarAbeDeployDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!Transform(ent).Anchored)
        {
            Appearance.SetData(ent, StellarAbeVisuals.Visuals, StellarAbeState.Undeployed);
        }
        else
        {
            Appearance.SetData(ent, StellarAbeVisuals.Visuals, StellarAbeState.Deployed);
        }

        ent.Comp.DoAfterId = null;
        Dirty(ent);
    }

    protected virtual void OnAbeMenu(Entity<StellarAnomalyAbeComponent> ent, ref StellarAbeRadialMessage args)
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
                StabilizerBeam(ent, args.BeamType, args.Actor);
                return;
        }
    }

    private void OnAnchorActivateInWorld(Entity<StellarRealityAnchorComponent> ent, ref ActivateInWorldEvent args)
    {
        // Launch DoAfter to turn the thing off.
        //
    }

    private void OnAnchorInteractHand(Entity<StellarRealityAnchorComponent> ent, ref InteractHandEvent args)
    {
        // Launch DoAfter to turn the thing off.
        //
    }

    #region Examine
    private void OnApeExamined(Entity<StellarAnomalyAbeComponent> ent, ref ExaminedEvent args)
    {
        // Examine Ape
    }

    private void OnAnomalyExamined(Entity<StellarAnomalyComponent> ent, ref ExaminedEvent args)
    {
        // Examine Ape
    }

    private void OnAnchorExamined(Entity<StellarRealityAnchorComponent> ent, ref ExaminedEvent args)
    {
        // Examine Ape
    }

    private void OnCapsuleExamined(Entity<StellarContainmentCapsuleComponent> ent, ref ExaminedEvent args)
    {
        // Examine Ape
    }
    #endregion
}

[Serializable, NetSerializable]
public sealed partial class StellarAbeBeamDoAfter : DoAfterEvent
{
    [DataField] public Vector2 MapDirection;

    [DataField] public EntProtoId? BeamToUse;

    public StellarAbeBeamDoAfter(Vector2 mapDirection, EntProtoId? beamToUse)
    {
        MapDirection = mapDirection;
        BeamToUse = beamToUse;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class StellarAbeDeployDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StellarAnchorDisableDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StellarAnchorAutoEnableDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class StellarAbeRadialMessage(AbeMenuMethod method, EntProtoId? beamType = null) : BoundUserInterfaceMessage
{
    public AbeMenuMethod Method = method;

    public EntProtoId? BeamType = beamType;
}

[Serializable, NetSerializable]
public enum AbeMenuMethod : byte
{
    Deploy,
    Pull,
    Rotate,
    ShootBeam,
}
