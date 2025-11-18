// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Containers.ItemSlots;
using Content.Stellar.Shared.WayfinderSign;
using Robust.Client.GameObjects;

namespace Content.Stellar.Client.WayfinderSign;

public sealed class WayfinderVisualsSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WayfinderLabelComponent, AppearanceChangeEvent>(OnLabelAppearanceChanged);
        SubscribeLocalEvent<WayfinderSignComponent, AppearanceChangeEvent>(OnSignAppearanceChanged);
    }

    private void OnSignAppearanceChanged(Entity<WayfinderSignComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is null || args.Sprite.Icon is null)
            return;
        _appearance.TryGetData(ent, WayfinderSignLayers.Slot1, out bool label1, args.Component);
        _appearance.TryGetData(ent, WayfinderSignLayers.Slot2, out bool label2, args.Component);
        _appearance.TryGetData(ent, WayfinderSignLayers.Slot3, out bool label3, args.Component);

        _sprite.LayerSetVisible((ent, args.Sprite), WayfinderSignLayers.Slot1, label1);
        _sprite.LayerSetVisible((ent, args.Sprite), WayfinderSignLayers.Slot1Arrow, label1);
        _sprite.LayerSetVisible((ent, args.Sprite), WayfinderSignLayers.Slot2, label2);
        _sprite.LayerSetVisible((ent, args.Sprite), WayfinderSignLayers.Slot2Arrow, label2);
        _sprite.LayerSetVisible((ent, args.Sprite), WayfinderSignLayers.Slot3, label3);
        _sprite.LayerSetVisible((ent, args.Sprite), WayfinderSignLayers.Slot3Arrow, label3);

        if (_itemSlots.TryGetSlot(ent, ent.Comp.Slot1ID, out var slot1) && TryComp<WayfinderLabelComponent>(slot1.Item, out var item1))
        {
            _sprite.LayerSetRsiState((ent, args.Sprite), WayfinderSignLayers.Slot1, $"{item1.LabelType}");
            _sprite.LayerSetRsiState((ent, args.Sprite), WayfinderSignLayers.Slot1Arrow, $"arrow-{item1.LabelDirection}");
        }

        if (_itemSlots.TryGetSlot(ent, ent.Comp.Slot2ID, out var slot2) && TryComp<WayfinderLabelComponent>(slot2.Item, out var item2))
        {
            _sprite.LayerSetRsiState((ent, args.Sprite), WayfinderSignLayers.Slot2, $"{item2.LabelType}");
            _sprite.LayerSetRsiState((ent, args.Sprite), WayfinderSignLayers.Slot2Arrow, $"arrow-{item2.LabelDirection}");
        }

        if (_itemSlots.TryGetSlot(ent, ent.Comp.Slot3ID, out var slot3) && TryComp<WayfinderLabelComponent>(slot3.Item, out var item3))
        {
            _sprite.LayerSetRsiState((ent, args.Sprite), WayfinderSignLayers.Slot3, $"{item3.LabelType}");
            _sprite.LayerSetRsiState((ent, args.Sprite), WayfinderSignLayers.Slot3Arrow, $"arrow-{item3.LabelDirection}");
        }
    }

    private void OnLabelAppearanceChanged(Entity<WayfinderLabelComponent> ent, ref AppearanceChangeEvent args)
    {
        _sprite.LayerSetRsiState((ent, args.Sprite), WayfinderLabelVisuals.Label, $"arrow-{ent.Comp.LabelDirection}");
    }
}
