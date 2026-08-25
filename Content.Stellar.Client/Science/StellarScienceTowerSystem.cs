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
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Science;

public sealed class StellarScienceTowerSystem : SharedStellarScienceTowerSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly SpriteSpecifier _completionOverlay = new SpriteSpecifier.Rsi(new("/Textures/_ST/Icons/icons-generic.rsi"), "check");
    private readonly ResPath _rsiPath = new("/Textures/_ST/Icons/radial-icons-sensortower.rsi");
    private readonly EntProtoId _popupBase = "StellarEffectCraftingPopup";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<StellarSensorTerminalCodeEvent>(OnTerminalCode);
        SubscribeNetworkEvent<StellarTowerCompleteVisualsEvent>(OnTowerCompleteEvent);
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

    private void OnTowerCompleteEvent(StellarTowerCompleteVisualsEvent args)
    {
        var ent = GetEntity(args.Target);
        var comp = Comp<StellarSensorTowerComponent>(ent);
        var popupEnt = Spawn(_popupBase, Transform(ent).Coordinates);
        var spriteComp = Comp<SpriteComponent>(popupEnt);
        var temp = _entMan.SpawnEntity(comp.FullDrive, MapCoordinates.Nullspace); // This is kinda evil. But it's also very easy and fast.

        _sprite.CopySprite(temp, popupEnt);
        _sprite.SetDrawDepth(popupEnt, (int) Content.Shared.DrawDepth.DrawDepth.Effects);
        spriteComp.LayerSetShader(_sprite.AddLayer((popupEnt, spriteComp), _completionOverlay), "unshaded");

        if (!_animation.HasRunningAnimation(popupEnt, "popup-effect"))
            _animation.Play(popupEnt, StellarAnimLib.SymbolPopup(0.5f, 4f), "popup-effect");

        QueueDel(temp); // Nobody will ever know, anyway!
    }
}

public enum SensorPopupVisuals : byte
{
    Key,
}
