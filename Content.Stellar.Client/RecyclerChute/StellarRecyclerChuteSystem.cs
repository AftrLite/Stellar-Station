// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Stellar.Shared.RecyclerChute;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Stellar.Client.RecyclerChute;

public sealed class StellarRecyclerChuteSystem : SharedStellarRecyclerChuteSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animPlayer = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<StellarChuteAnimEvent>(OnAnim);
    }

    private void OnAnim(StellarChuteAnimEvent args)
    {
        var ent = GetEntity(args.Target);

        if (args.Remove == true)
        {
            _sprite.SetVisible(ent, true);
            return;
        }

        if (Player.LocalEntity != ent)
        {
            _sprite.SetVisible(ent, false);
            return;
        }

        var travelAnim = TravelAnim((float) args.TravelTime.TotalSeconds);
        _animPlayer.Play(ent, travelAnim, "travel-effect");
    }

    private Animation TravelAnim(float duration)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(duration) + TimeSpan.FromSeconds(0.05),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0, 7.5f), 0f, Easings.OutBounce),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, duration*0.3f, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, duration*0.4f, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0, -8f), duration*0.3f, Easings.InSine),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                    },
                },

                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180), duration/4),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180*2), duration/4),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180*3), duration/4),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(180*4), duration/4),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0f),
                    },
                },
            },
        };
    }

    private Animation OffsetOthersAnim(float duration)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(duration) + TimeSpan.FromSeconds(0.05),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), duration),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), 0f),
                    },
                },
            },
        };
    }
}
