// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Client.Animations;
using Content.Shared.Hands;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Content.Stellar.Shared.Social;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Social;

/// <inheritdoc/>
public sealed class StellarSocialSystem : SharedStellarSocialSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animPlayer = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly EntProtoId _socialEffectBase = "StellarSocialVisualsEffect";
    private readonly SpriteSpecifier _socialRequestOverlay = new SpriteSpecifier.Rsi(new("/Textures/_ST/Icons/interaction-radial-icons.rsi"), "query");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<StellarSocialRequestEvent>(OnSocialRequest);
        SubscribeNetworkEvent<StellarCoopEmoteVisualsEvent>(OnEmoteVisuals);
        SubscribeNetworkEvent<StellarTransferItemVisualsEvent>(OnTransferVisuals);
    }

    private void RemoveRequestEffect(Entity<StellarSocialComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (ent.Comp.RequestEffect == null || TerminatingOrDeleted(ent.Comp.RequestEffect) || _animPlayer.HasRunningAnimation(ent.Comp.RequestEffect.Value, "request-fadeout"))
            return;

        var requestAnim = FadeRequestAnim();
        _animPlayer.Stop(ent.Comp.RequestEffect.Value, "request-effect");
        _animPlayer.Play(ent.Comp.RequestEffect.Value, requestAnim, "request-fadeout");
        ent.Comp.RequestEffect = null;
    }

    protected override void OnItemDeselected(Entity<StellarOfferedItemComponent> ent, ref HandDeselectedEvent args)
    {
        RemoveRequestEffect(args.User);
        base.OnItemDeselected(ent, ref args);
    }

    protected override void OnMove(Entity<StellarSocialComponent> ent, ref MoveInputEvent args)
    {
        if ((args.Entity.Comp.HeldMoveButtons & (MoveButtons.Down | MoveButtons.Left | MoveButtons.Up | MoveButtons.Right)) == 0x0)
            return;

        RemoveRequestEffect((ent.Owner, ent.Comp));
        base.OnMove(ent, ref args);
    }

    private void OnSocialRequest(StellarSocialRequestEvent args)
    {
        var requestee = GetEntity(args.Requestee);
        var target = GetEntity(args.Target);

        if (!TryComp<StellarSocialComponent>(requestee, out var socialComp))
            return;

        var requestAnim = SocialRequestAnim((float) socialComp.TimeoutTime.TotalSeconds);
        var requestEnt = Spawn(socialComp.RequestVfxEnt, Transform(requestee).Coordinates);
        var track = EnsureComp<TrackUserComponent>(requestEnt);
        var spriteComp = Comp<SpriteComponent>(requestEnt);
        socialComp.RequestEffect = requestEnt;
        track.User = requestee;

        if (socialComp.OfferedItem != null)
        {
            _sprite.CopySprite(socialComp.OfferedItem.Value, (requestEnt, spriteComp));
            spriteComp.LayerSetShader(_sprite.AddLayer((requestEnt, spriteComp), _socialRequestOverlay), "unshaded");
        }
        else if (socialComp.RequestedEmote != null && _proto.Index(socialComp.RequestedEmote) is { } proto && proto.Icon != null)
        {
            _sprite.LayerSetSprite((requestEnt, spriteComp), 0, proto.Icon);
            _sprite.LayerSetColor((requestEnt, spriteComp), 0, Color.White.WithAlpha(0.5f));
        }

        _sprite.SetDrawDepth(requestEnt, (int) Content.Shared.DrawDepth.DrawDepth.Effects);
        _audio.PlayEntity(socialComp.RequestSfx, target, requestee, AudioParams.Default.WithVariation(0.05f));
        _animPlayer.Play(requestEnt, requestAnim, "request-effect");
    }

    private void OnEmoteVisuals(StellarCoopEmoteVisualsEvent args)
    {
        var requestee = GetEntity(args.Requestee);
        var target = GetEntity(args.Target);

        if (!TryComp<StellarSocialComponent>(requestee, out var socialComp) || !_proto.TryIndex(socialComp.RequestedEmote, out var proto))
            return;

        RemoveRequestEffect((requestee, socialComp));

        var dist = PositionBetweenPlayers(requestee, target) / 2;
        var effectEnt = Spawn(proto.VfxEntity, Transform(requestee).Coordinates);

        _audio.PlayEntity(proto.EmoteSound, effectEnt, effectEnt);
        PopUp.PopupClient(Loc.GetString(proto.PopUpSuccess, ("target", target)), requestee, requestee);
        PopUp.PopupClient(Loc.GetString(proto.PopUpSuccess, ("target", requestee)), target, target);

        var moveRequestee = MovePlayerAnim(dist);
        var moveTarget = MovePlayerAnim(-dist);
        var emoteAnim = EmoteVisualsAnim(1.5f, dist);
        _animPlayer.Play(effectEnt, emoteAnim, "emote-effect"); // Emote visual popup.
        if (!_animPlayer.HasRunningAnimation(requestee, "emote-player-nudge")) // Make the players nudge towards each other.
            _animPlayer.Play(requestee, moveRequestee, "emote-player-nudge");
        if (!_animPlayer.HasRunningAnimation(target, "emote-player-nudge"))
            _animPlayer.Play(target, moveTarget, "emote-player-nudge");
        socialComp.RequestEffect = null;
    }

    private void OnTransferVisuals(StellarTransferItemVisualsEvent args)
    {
        var requestee = GetEntity(args.Requestee);
        var target = GetEntity(args.Target);
        var item = GetEntity(args.Item);

        RemoveRequestEffect(requestee);

        PopUp.PopupClient(Loc.GetString("stellar-social-item-give", ("target", target), ("item", args.ItemName)), requestee, requestee);
        PopUp.PopupClient(Loc.GetString("stellar-social-item-take", ("target", requestee), ("item", args.ItemName)), target, target);

        var dist = PositionBetweenPlayers(requestee, target);
        var transferEnt = Spawn(_socialEffectBase, Transform(requestee).Coordinates);
        var transferAnim = TransferItemAnim(dist);

        _sprite.CopySprite(item, transferEnt);
        _sprite.SetDrawDepth(transferEnt, (int) Content.Shared.DrawDepth.DrawDepth.Effects);
        _transform.SetWorldRotationNoLerp(transferEnt, dist.ToAngle());
        _animPlayer.Play(transferEnt, transferAnim, "item-transfer"); // we use the same Animation Key as the request fadeout, since the Item Transfer Animation utilizes the request entity.
    }

    private Vector2 PositionBetweenPlayers(EntityUid start, EntityUid end)
    {
        var performerXform = Transform(start);
        var targetXform = Transform(end);
        if (performerXform.MapID == MapId.Nullspace || targetXform.MapID == MapId.Nullspace)
            return Vector2.Zero;

        if (performerXform.ParentUid != targetXform.ParentUid)
            return Vector2.Zero;

        return targetXform.LocalPosition - performerXform.LocalPosition;
    }

    # region Animation
    private static Animation SocialRequestAnim(float animTime)
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
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.5f), 0f, Easings.InOutQuad),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.8f), animTime * 0.05f, Easings.InOutSine),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f, animTime * 0.025f), 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), animTime * 0.05f, Easings.OutElastic),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.75f, 0.75f), animTime * 0.9925f),
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
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), animTime * 0.025f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), animTime * 0.9f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), animTime * 0.025f),
                    },
                },
            },
        };
    }

    private static Animation FadeRequestAnim()
    {
        return new Animation()
        {
            Length = TimeSpan.FromSeconds(10),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Color.White, 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0), 0.25f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0), 9.25f),
                    },
                },
            },
        };
    }

    private static Animation EmoteVisualsAnim(float animTime, Vector2 dist)
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
                        new AnimationTrackProperty.KeyFrame(dist + new Vector2(0f, 0.5f), 0f),
                        new AnimationTrackProperty.KeyFrame(dist + new Vector2(0f, 0.8f), 0.5f, Easings.InOutSine),
                        new AnimationTrackProperty.KeyFrame(dist + new Vector2(0f, 0.9f), 1.5f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Color),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), 0.25f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), 1f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0.25f),
                    },
                },
            },
        };
    }

    private static Animation TransferItemAnim(Vector2 dist)
    {
        const float length = 10f; // Hog up the same amount of time as the request fadeout.

        return new Animation
        {
            Length = TimeSpan.FromSeconds(length),
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
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), 0.1f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(1f), 0.2f),
                        new AnimationTrackProperty.KeyFrame(Color.White.WithAlpha(0f), 0.25f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Scale),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.25f, 0.25f), 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(1f, 1f), 0.25f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0.5f, 0.5f), 0.15f),
                    },
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0f), 0.1f, Easings.InOutBack),
                        new AnimationTrackProperty.KeyFrame(dist, 0.2f, Easings.InSine),
                    },
                },
            },
        };
    }

    private static Animation MovePlayerAnim(Vector2 dist)
    {
        const float length = 0.5f;

        return new Animation
        {
            Length = TimeSpan.FromSeconds(length),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f, Easings.OutSine),
                        new AnimationTrackProperty.KeyFrame(dist * 0.66f, length*0.4f, Easings.InOutCirc),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, length*0.6f, Easings.OutBack),
                    },
                },
            },
        };
    }
    #endregion
}
