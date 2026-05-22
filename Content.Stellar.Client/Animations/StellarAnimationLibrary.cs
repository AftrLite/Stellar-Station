// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using JetBrains.Annotations;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Map;

namespace Content.Stellar.Client.Animations;

/// <summary>
/// A big bunch of helper methods for handling positions and animations. Saves on code duplication!
/// When using this library, keep in mind that all timings are assumed to be based on floats-to-seconds, where 1f is one second.
/// Animations' frame timings are divided across input time provided.
/// </summary>
public static class StellarAnimLib
{
    [PublicAPI]
    public static Vector2 PositionOffset(TransformComponent start, TransformComponent end)
    {
        if (start .MapID == MapId.Nullspace || end .MapID == MapId.Nullspace)
            return Vector2.Zero;

        if (start .ParentUid != end .ParentUid)
            return Vector2.Zero;

        return end .LocalPosition - start .LocalPosition;
    }

    [PublicAPI]
    public static Animation FadeSimple(float animTime, float delay = 0f, float startOpacity = 0f, float endOpacity = 1f)
    {
        return new Animation()
        {
            Length = TimeSpan.FromSeconds(animTime + delay),

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(startOpacity), 0f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(startOpacity), delay),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(startOpacity), 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(endOpacity), animTime),
                    },
                },
            },
        };
    }

    [PublicAPI]
    public static Animation FadeSimpleWithLight(float animTime, float delay = 0f, float startOpacity = 0f, float endOpacity = 1f, float startEnergy = 1f, float endEnergy = 0f)
    {
        return new Animation()
        {
            Length = TimeSpan.FromSeconds(animTime + delay),

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(startOpacity), 0f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(startOpacity), delay),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(startOpacity), 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(endOpacity), animTime),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(PointLightComponent),
                    Property = nameof(PointLightComponent.Energy),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(startEnergy, delay),
                        new AnimationTrackProperty.KeyFrame(startEnergy, 0f),
                        new AnimationTrackProperty.KeyFrame(endEnergy, animTime),
                    },
                },
            },
        };
    }

    [PublicAPI]
    public static Animation SymbolPopup(float start, float end, float delay = 0)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(start + end + delay),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.5f), delay),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.5f), 0f, Easings.InOutQuad),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.75f), start, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.5f), end * 0.9f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0f), end * 0.1f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f, 9f * 0.025f), delay),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f, 9f * 0.025f), 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), start, Easings.OutElastic),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), end * 0.9f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f), end * 0.1f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), start),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0.66f), end * 0.45f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0.33f), end * 0.45f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), end * 0.1f),
                    },
                },
            },
        };
    }

    [PublicAPI]
    public static Animation KnockbackFacing(float animTime, Angle facing, Vector2 offset)
    {
        var offsetFromCurrent = facing.Opposite().ToWorldVec() * 0.15f;

        return new Animation()
        {
            Length = TimeSpan.FromSeconds(animTime),

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(offset, 0f),
                        new AnimationTrackProperty.KeyFrame(offset + offsetFromCurrent, animTime * 0.6f, Easings.OutExpo),
                        new AnimationTrackProperty.KeyFrame(offset, animTime * 0.3f, Easings.OutCirc),
                    },
                },
            },
        };
    }

    [PublicAPI]
    public static Animation KnockbackClampedRelative(Vector2 offsetInput, float animTime, float delay = 0f)
    {
        var offsetY = Math.Clamp(offsetInput.Y * 2, -2, 2);
        var offsetX = Math.Clamp(offsetInput.X * 2, -2, 2);
        var offset = new Vector2(offsetX, offsetY);

        return new Animation
        {
            Length = TimeSpan.FromSeconds(animTime + delay),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, delay),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(-offset * 0.2f, animTime*0.4f, Easings.InOutCirc),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, animTime*0.6f, Easings.OutBack),
                    },
                },
            },
        };
    }

    [PublicAPI]
    public static Animation KnockbackClampedRelative(TransformComponent start, TransformComponent end, float animTime, float delay = 0f)
    {
        var offsetInput = PositionOffset(start, end);
        var offsetY = Math.Clamp(offsetInput.Y * 2, -2, 2);
        var offsetX = Math.Clamp(offsetInput.X * 2, -2, 2);
        var offset = new Vector2(offsetX, offsetY);

        return new Animation
        {
            Length = TimeSpan.FromSeconds(animTime + delay),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, delay),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(-offset * 0.2f, animTime*0.4f, Easings.InOutCirc),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, animTime*0.6f, Easings.OutBack),
                    },
                },
            },
        };
    }

    [PublicAPI]
    public static Animation ElasticBounce(float animTime, float delay = 0f)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(animTime + delay),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), delay),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0.5f), animTime * 0.1f, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), animTime * 0.9f, Easings.OutElastic),
                    },
                },
            },
        };
    }

    [PublicAPI]
    public static Animation ProjectileBase(float animTime)
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
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0.5f), animTime * 0.025f, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), animTime * 0.825f, Easings.OutElastic),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f, 0.5f), animTime * 0.1f, Easings.InOutSine),
                    },
                },
            },
        };
    }
}

