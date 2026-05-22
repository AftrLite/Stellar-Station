// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
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
public sealed class SharedStellarScienceAnchorSystem : EntitySystem
{
    // [Dependency] protected readonly PullingSystem Pull = default!;
    // [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    // [Dependency] protected readonly SharedAudioSystem Audio = default!;
    // [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    // [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    // [Dependency] protected readonly SharedUserInterfaceSystem UiSystem = default!;
    //
    // [Dependency] private readonly IPrototypeManager _proto = default!;
    // [Dependency] private readonly IRobustRandom _random = default!;
    // [Dependency] private readonly SharedChatSystem _chat = default!;
    // [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarRealityAnchorComponent, ActivateInWorldEvent>(OnAnchorActivateInWorld);
        SubscribeLocalEvent<StellarRealityAnchorComponent, InteractHandEvent>(OnAnchorInteractHand);
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
}

// [Serializable, NetSerializable]
// public sealed partial class StellarAnchorDisableDoAfter : SimpleDoAfterEvent;

// [Serializable, NetSerializable]
// public sealed partial class StellarAnchorAutoEnableDoAfter : SimpleDoAfterEvent;
