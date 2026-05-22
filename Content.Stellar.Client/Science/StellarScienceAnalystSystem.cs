// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Stellar.Client.Animations;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

public sealed class StellarScienceAnalystSystem : SharedStellarScienceAnalystSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
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
        if (!TryComp<StellarSensorTerminalComponent>(ent, out var comp) || comp.PopupEffect == null || TerminatingOrDeleted(comp.PopupEffect))
            return;

        var codePopup = comp.PopupEffect.Value;
        if (!args.End)
        {
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

            _animation.Stop(codePopup, "popup-effect");
            _animation.Play(codePopup, StellarAnimLib.SymbolPopup(0.5f, 39.5f), "popup-effect");
            return;
        }

        _animation.Stop(codePopup, "popup-effect");
        _animation.Play(codePopup, StellarAnimLib.FadeSimpleWithLight(0.9f, 0f, 0.5f, 0f, 3f), "popup-effect");
        comp.PopupEffect = null;
    }
}

public enum SensorPopupVisuals : byte
{
    Key,
}
