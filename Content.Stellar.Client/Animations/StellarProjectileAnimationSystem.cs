// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ES.Core.Timer.Components;
using Content.Stellar.Shared.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Spawners;

namespace Content.Stellar.Client.Animations;

/// <summary>
/// The most basic system ever. I made this purely to save on component and code duplication.
/// Cleans up entities once they've finished their animations.
/// No, this does not clean up entities on the server. Therefore, please make sure entities also have TimedDespawnComponent if you're using this with entities that also exist on the server.
/// Or use a Timing method to tidy them up. That works too, but it's gross.
/// </summary>
public sealed class StellarProjectileAnimationSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarProjectileAnimationComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(Entity<StellarProjectileAnimationComponent> ent, ref ComponentInit args)
    {
        var time = (float) ent.Comp.Duration.TotalSeconds;
        if (ent.Comp.UseTimeDespawn && TryComp<ESTimedDespawnComponent>(ent, out var despawn))
            time = (float) despawn.Lifetime.TotalSeconds;

        _animation.Play(ent, StellarAnimLib.ProjectileBase(time), ent.Comp.AnimateKey);
    }
}
