// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Construction.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science;

public abstract class SharedStellarScienceAnomalyTechSystem : EntitySystem
{
    [Dependency] protected readonly IRobustRandom Random = default!;

    [Dependency] private readonly PullingSystem _pull = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyApeComponent, StellarApeRadialMessage>(OnApeMenu);

        SubscribeLocalEvent<StellarRealityAnchorComponent, ActivateInWorldEvent>(OnAnchorActivateInWorld);
        SubscribeLocalEvent<StellarRealityAnchorComponent, InteractHandEvent>(OnAnchorInteractHand);

        SubscribeLocalEvent<StellarAnomalyApeComponent, ActivateInWorldEvent>(OnApeActivateInWorld);
        SubscribeLocalEvent<StellarAnomalyApeComponent, InteractHandEvent>(OnApeInteractHand);

        SubscribeLocalEvent<StellarAnomalyApeComponent, ExaminedEvent>(OnApeExamined);
        SubscribeLocalEvent<StellarAnomalyComponent, ExaminedEvent>(OnAnomalyExamined);
        SubscribeLocalEvent<StellarRealityAnchorComponent, ExaminedEvent>(OnAnchorExamined);
        SubscribeLocalEvent<StellarContainmentCapsuleComponent, ExaminedEvent>(OnCapsuleExamined);
    }

    private void OnApeMenu(Entity<StellarAnomalyApeComponent> ent, ref StellarApeRadialMessage args)
    {
        switch (args.Method)
        {
            case ApeMenuMethod.Anchor:
                if (Transform(ent).Anchored)
                    _transform.Unanchor(ent);
                else
                {
                    _transform.AnchorEntity(ent);
                    if (TryComp<PullableComponent>(ent, out var pullable))
                        _pull.TryStopPull(ent, pullable);
                }
                break;
            case ApeMenuMethod.Pull:
                _pull.TogglePull(ent.Owner, args.Actor);
                break;
            case ApeMenuMethod.Rotate:
                _transform.SetLocalRotation(ent.Owner, Transform(ent).LocalRotation + Angle.FromDegrees(90));
                break;
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

    private void OnApeActivateInWorld(Entity<StellarAnomalyApeComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.Handled)
            return;

        _ui.OpenUi(ent.Owner, StellarApeRadialKey.Key, args.User, true);
    }

    private void OnApeInteractHand(Entity<StellarAnomalyApeComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        _ui.OpenUi(ent.Owner, StellarApeRadialKey.Key, args.User, true);
    }

    #region Examine
    private void OnApeExamined(Entity<StellarAnomalyApeComponent> ent, ref ExaminedEvent args)
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
public sealed partial class StellarAnchorDisableDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class StellarAnchorAutoEnableDoAfter : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class StellarApeRadialMessage(ApeMenuMethod method) : BoundUserInterfaceMessage
{
    public ApeMenuMethod Method = method;
}

[Serializable, NetSerializable]
public enum ApeMenuMethod : byte
{
    Anchor,
    Pull,
    Rotate,
    ShootAlpha,
    ShootBeta,
    ShootGamma,
    ShootSigma,
}
