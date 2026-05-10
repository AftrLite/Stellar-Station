// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Stellar.Shared.Science;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

public sealed class StellarScienceAnalystSystem : SharedStellarScienceAnalystSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animPlayer = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly ResPath _rsiPath = new("/Textures/_ST/Icons/radial-icons-sensortower.rsi");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<StellarSensorTerminalCodeEvent>(OnTerminalCode);
    }

    private void OnTerminalCode(StellarSensorTerminalCodeEvent args)
    {
        var ent = GetEntity(args.Target);
        if (!TryComp<StellarSensorTerminalComponent>(ent, out var comp) || comp.PopupEffect == null)
            return;

        var codePopup = comp.PopupEffect.Value;

        if (TerminatingOrDeleted(codePopup))
            return;

        if (!args.End)
        {
            var popupAnim = CodePopupAnim(40);
            var sprite = Comp<SpriteComponent>(codePopup);
            var offset = new Vector2(-1.25f - 0.09375f, 0);

            foreach (var character in comp.TowerCode)
            {
                var layer = _sprite.AddLayer((codePopup, sprite), new SpriteSpecifier.Rsi(_rsiPath, $"{character}"));
                _sprite.LayerMapSet((codePopup, sprite), SensorPopupVisuals.Key, layer);
                _sprite.LayerSetOffset((codePopup, sprite), layer, offset);
                sprite.LayerSetShader(SensorPopupVisuals.Key, "unshaded");
                offset += new Vector2(0.5f + 0.03125f, 0);
            }

            _animPlayer.Stop(codePopup, "popup-effect");
            _animPlayer.Play(codePopup, popupAnim, "popup-effect");
            return;
        }

        _animPlayer.Stop(codePopup, "popup-effect");
        _animPlayer.Play(codePopup, FadeAnim(), "popup-effect");
        comp.PopupEffect = null;
    }

    #region Animation
    private static Animation CodePopupAnim(float animTime)
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(animTime),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0.5f), 0f, Easings.InOutQuad),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0.75f), 9f * 0.05f, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0.5f), animTime * 0.9f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0f), animTime * 0.025f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f, 9f * 0.025f), 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), 9f * 0.05f, Easings.OutElastic),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), animTime * 0.9f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f), animTime * 0.025f),
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
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), 9f * 0.025f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0.66f), animTime * 0.45f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0.33f), animTime * 0.45f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), animTime * 0.025f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(PointLightComponent),
                    Property = nameof(PointLightComponent.Energy),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(0f, 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(3f, 9f * 0.025f),
                        new AnimationTrackProperty.KeyFrame(3f, animTime * 0.9f),
                        new AnimationTrackProperty.KeyFrame(0f, animTime * 0.025f),
                    },
                },
            },
        };
    }

    private static Animation FadeAnim()
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(1),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0.5f), 0.5f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0.4f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(PointLightComponent),
                    Property = nameof(PointLightComponent.Energy),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(3f, 0.5f),
                        new AnimationTrackProperty.KeyFrame(0f, 0.4f),
                    },
                },
            },
        };
    }
    #endregion
}

public enum SensorPopupVisuals : byte
{
    Key,
}
