// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.Animations;
using Robust.Client.GameObjects;

namespace Content.Stellar.Client.Animations;

/// <summary>
/// The most basic system ever. I made this purely to save on component and code duplication.
/// Cleans up entities once they've finished their animations.
/// No, this does not clean up entities on the server. Therefore, please make sure entities also have TimedDespawnComponent if you're using this with entities that also exist on the server.
/// </summary>
public sealed class StellarDeleteOnAnimateSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarDeleteOnAnimateComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }

    private void OnAnimationCompleted(Entity<StellarDeleteOnAnimateComponent> ent, ref AnimationCompletedEvent args)
    {
        if (!ent.Comp.DeleteOnStop && args.Finished)
            PredictedQueueDel(ent);
        if (ent.Comp.DeleteOnStop)
            PredictedQueueDel(ent);
    }
}
