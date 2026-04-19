// SPDX-FileCopyrightText: 2026 Janet Blackquill <uhhadd@gmail.com>
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Client.Effects;
using Content.Shared.Effects;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Stellar.Client.Interaction;

public sealed class StellarInteractionRecoilSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;

    private const string AnimateKey = "twitch-animation";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<ColorFlashEffectEvent>(OnColorFlashEffect);
    }

    private Animation GetAnimation(Angle facing, Vector2 offset)
    {
        const float offsetDistance = 4f / EyeManager.PixelsPerMeter;

        var offsetFromCurrent = facing.Opposite().ToWorldVec() * offsetDistance;
        var offsetLength = TimeSpan.FromMilliseconds(175);
        var returnLength = TimeSpan.FromMilliseconds(87.5);

        return new Animation()
        {
            Length = offsetLength + returnLength,

            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(offset, 0f),
                        new AnimationTrackProperty.KeyFrame(offset + offsetFromCurrent, (float)offsetLength.TotalSeconds, Easings.OutExpo),
                        new AnimationTrackProperty.KeyFrame(offset, (float)returnLength.TotalSeconds, Easings.OutCirc),
                    },
                },
            },
        };
    }

    private void Recoil(EntityUid entity)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite) || !HasComp<StellarInteractionRecoilTargetComponent>(entity))
            return;

        if (_animation.HasRunningAnimation(entity, AnimateKey))
            return;

        _animation.Play(entity, GetAnimation(Transform(entity).LocalRotation, sprite.Offset), AnimateKey);
    }

    private void OnColorFlashEffect(ColorFlashEffectEvent ev)
    {
        foreach (var netEntity in ev.Entities)
        {
            var entity = GetEntity(netEntity);

            var targetEv = new GetFlashEffectTargetEvent(entity);
            RaiseLocalEvent(entity, ref targetEv);

            Recoil(targetEv.Target);
        }
    }
}
