// SPDX-FileCopyrightText: 2026 Janet Blackquill <uhhadd@gmail.com>
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Client.Effects;
using Content.Shared.Effects;
using Content.Stellar.Client.Animations;
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

    private void Recoil(EntityUid entity)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite) || !HasComp<StellarInteractionRecoilTargetComponent>(entity))
            return;

        if (_animation.HasRunningAnimation(entity, AnimateKey))
            return;

        _animation.Play(entity, StellarAnimLib.KnockbackFacing(0.25f, Transform(entity).LocalRotation, sprite.Offset), AnimateKey);
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
