// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Stellar.Client.Animations;
using Content.Stellar.Shared.Crafting;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Crafting;

public sealed class StellarCraftingSystem : SharedStellarCraftingSystem
{
    [Dependency] private readonly IEntityManager _entMan = default!;

    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    private readonly EntProtoId _popupBase = "StellarEffectCraftingPopup";
    private readonly SpriteSpecifier _completionOverlay = new SpriteSpecifier.Rsi(new("/Textures/_ST/Icons/icons-generic.rsi"), "check");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarCraftingBenchComponent, ActivateInWorldEvent>(OnActivateInWorld);

        SubscribeNetworkEvent<StellarCraftingVisualsEvent>(OnCraftingVisuals);
    }

    private void OnCraftingVisuals(StellarCraftingVisualsEvent args)
    {
        var ent = GetEntity(args.Target);
        var comp = Comp<StellarCraftingBenchComponent>(ent);
        var popupEnt = Spawn(_popupBase, Transform(ent).Coordinates);
        var spriteComp = Comp<SpriteComponent>(popupEnt);
        var temp = _entMan.SpawnEntity(comp.Output, MapCoordinates.Nullspace); // This is kinda evil. But it's also very easy and fast.

        _sprite.CopySprite(temp, popupEnt);
        _sprite.SetDrawDepth(popupEnt, (int) Content.Shared.DrawDepth.DrawDepth.Effects);
        spriteComp.LayerSetShader(_sprite.AddLayer((popupEnt, spriteComp), _completionOverlay), "unshaded");

        if (!_animation.HasRunningAnimation(popupEnt, "popup-effect"))
            _animation.Play(popupEnt, StellarAnimLib.SymbolPopup(0.5f, 4f), "popup-effect");

        QueueDel(temp); // Nobody will ever know, anyway!
    }


    private void OnActivateInWorld(Entity<StellarCraftingBenchComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.Handled)
            return;

        HandleUI(ent, args.User);
    }

    protected override void OnInteractHand(Entity<StellarCraftingBenchComponent> ent, ref InteractHandEvent args)
    {
        base.OnInteractHand(ent, ref args);
        if (args.Handled)
            return;

        HandleUI(ent, args.User);
    }

    private void HandleUI(Entity<StellarCraftingBenchComponent> ent, EntityUid user)
    {
        if (_doAfter.IsRunning(ent.Comp.DoAfterId) || !Power.IsPowered(ent.Owner) || _ui.IsUiOpen(ent.Owner, StellarCraftingKey.Key))
            return;

        if (ent.Comp.OutputStorage.Count > 0)
        {
            PopUp.PopupClient("Output slot is full!", user, PopupType.Medium);
            return;
        }

        _ui.OpenUi(ent.Owner, StellarCraftingKey.Key, user, true);
    }
}
