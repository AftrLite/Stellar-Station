// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.IdentityManagement;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Stellar.Shared.Science;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Map;

namespace Content.Stellar.Client.Science;

public sealed class StellarScienceAnomalySystem : SharedStellarScienceAnomalySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<StellarAnomalyReactionVisualsEvent>(OnAnomalyReaction);

    }

    private void OnAnomalyReaction(StellarAnomalyReactionVisualsEvent args)
    {
        var ent = GetEntity(args.Target);
        var source = GetEntity(args.Source);

        if (HasComp<StellarAnomalyComponent>(ent) && !_animation.HasRunningAnimation(ent, "react"))
        {
            var dist = PositionOffset(ent, source);
            var distY = Math.Clamp(dist.Y * 2, -2, 2);
            var distX = Math.Clamp(dist.X * 2, -2, 2);
            var reactAnim = AnomalyReactAnim(1.25f, new Vector2(distX, distY));
            _animation.Play(ent, reactAnim, "react");
        }
    }

    private Vector2 PositionOffset(EntityUid start, EntityUid end)
    {
        var startXform  = Transform(start);
        var endXform  = Transform(end);
        if (startXform .MapID == MapId.Nullspace || endXform .MapID == MapId.Nullspace)
            return Vector2.Zero;

        if (startXform .ParentUid != endXform .ParentUid)
            return Vector2.Zero;

        return endXform .LocalPosition - startXform .LocalPosition;
    }

    private static Animation AnomalyReactAnim(float animTime, Vector2 midPos)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(animTime),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        // new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), animTime * 0.1f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0.5f), animTime * 0.1f, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), animTime * 0.9f, Easings.OutElastic),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, animTime * 0.035f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(-midPos * 0.2f, animTime*0.4f, Easings.InOutCirc),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, animTime*0.6f, Easings.OutBack),
                    },
                },
            },
        };
    }
}
