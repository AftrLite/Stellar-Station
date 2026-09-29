// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared._ES.Core.Timer;
using Content.Stellar.Shared._ES.Core.Timer.Components;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Transition;

public abstract class SharedStellarTransitionSystem : EntitySystem
{
    [Dependency] private readonly ESEntityTimerSystem _timers = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, StellarDelayedTransitionEvent>(OnTransitionDelayed);
    }

    private void OnTransitionDelayed(Entity<ActorComponent> ent, ref StellarDelayedTransitionEvent args)
    {
        if (args.Target != null)
            RaiseNetworkEvent(new StellarTransitionEvent(args.Target.Value, args.Length));
    }

    public void DoTransition(EntityUid target, TimeSpan? delay = null, TimeSpan? length = null)
    {
        if (delay == null)
            RaiseNetworkEvent(new StellarTransitionEvent(GetNetEntity(target), length));
        else
            _timers.SpawnTimer(target, delay.Value, new StellarDelayedTransitionEvent(GetNetEntity(target), length));

    }
}

[NetSerializable, Serializable]
public sealed partial class StellarDelayedTransitionEvent : ESEntityTimerEvent
{
    public NetEntity? Target = null;

    public TimeSpan? Length = null;

    public StellarDelayedTransitionEvent()
    {

    }

    public StellarDelayedTransitionEvent(NetEntity target, TimeSpan? length = null)
    {
        Target = target;

        Length = length;
    }
}

[NetSerializable, Serializable]
public sealed partial class StellarTransitionEvent(NetEntity target, TimeSpan? length = null) : EntityEventArgs
{
    public NetEntity Target = target;

    public TimeSpan? Length = length;
}

