// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.Examine;
using Content.Stellar.Client.Animations;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

public sealed class StellarScienceAnomalySystem : SharedStellarScienceAnomalySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly ResPath _rsiPath = new("/Textures/_ST/Icons/icons-generic.rsi");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyComponent, ComponentInit>(OnComponentInit);
        SubscribeNetworkEvent<StellarAnomalyReactionVisualsEvent>(OnAnomalyReaction);
    }

    private void OnComponentInit(Entity<StellarAnomalyComponent> ent, ref ComponentInit args)
    {
        _animation.Play(ent, StellarAnimLib.FadeSimple(1f, 1.2f), "spawn-fade");
        _animation.Play(ent, StellarAnimLib.ElasticBounce(2.5f, 1.55f), "spawn-bounce");
    }

    private void OnAnomalyReaction(StellarAnomalyReactionVisualsEvent args)
    {
        var ent = GetEntity(args.Target);
        var source = GetEntity(args.Source);

        if (!TryComp<StellarAnomalyComponent>(ent, out var comp) || args.CodeCache == string.Empty)
            return;

        var codePopup = Spawn(comp.CodePopup, Transform(ent).Coordinates);
        var sprite = Comp<SpriteComponent>(codePopup);
        var length = args.CodeCache.Length;

        for (int i = 0; i < length; i++)
        {
            var layer = _sprite.AddLayer((codePopup, sprite), new SpriteSpecifier.Rsi(_rsiPath, "false"));
            var offset = new Vector2(((float)length/length * - length * length * 0.1f + i - 0.1f) * 0.66f, 0f); // the evil stupid dumb numbers
            if (length == 1)
                offset = new Vector2(-0.03125f, 0f);

            _sprite.LayerMapSet((codePopup, sprite), AnomalyPopupVisuals.Key, layer);
            _sprite.LayerSetOffset((codePopup, sprite), layer, offset);
            sprite.LayerSetShader(AnomalyPopupVisuals.Key, "unshaded");
            if (args.CodeCache[i].Equals(comp.AnomalyCode[i]))
                _sprite.LayerSetRsi((codePopup, sprite), layer, _rsiPath, "true");
        }

        if (!_animation.HasRunningAnimation(ent, "popup-effect"))
            _animation.Play(codePopup, StellarAnimLib.SymbolPopup(0.5f, 2f), "popup-effect");

        if (!_animation.HasRunningAnimation(ent, "react-bounce"))
            _animation.Play(ent, StellarAnimLib.ElasticBounce(1.25f), "react-bounce");

        if (!_animation.HasRunningAnimation(ent, "react-knockback"))
            _animation.Play(ent, StellarAnimLib.KnockbackClampedRelative(Transform(ent), Transform(source), 1.25f, 0.04375f), "react-knockback");
    }
}
