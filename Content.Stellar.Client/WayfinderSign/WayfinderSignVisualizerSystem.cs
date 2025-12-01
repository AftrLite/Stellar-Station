// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.WayfinderSign;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.WayfinderSign;

public sealed class WayfinderSignVisualizerSystem : VisualizerSystem<WayfinderSignVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, WayfinderSignVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (!AppearanceSystem.TryGetData<WayfinderSignSlotsAppearance>(uid, WayfinderSignLayers.Slots, out var slots, args.Component))
            return;

        foreach (var slot in slots.Slots.Values)
        {
            AppearanceSystem.TryGetData<SpriteSpecifier>(uid, slot.Base, out var @base, args.Component);
            AppearanceSystem.TryGetData<SpriteSpecifier>(uid, slot.Arrow, out var arrow, args.Component);

            SpriteSystem.LayerSetVisible((uid, args.Sprite), slot.Base, @base is not null);
            if (@base is not null)
                SpriteSystem.LayerSetSprite((uid, args.Sprite), slot.Base, @base);

            SpriteSystem.LayerSetVisible((uid, args.Sprite), slot.Arrow, arrow is not null);
            if (arrow is not null)
                SpriteSystem.LayerSetSprite((uid, args.Sprite), slot.Arrow, @arrow);

            if (AppearanceSystem.TryGetData<Direction>(uid, slot.Direction, out var direction, args.Component))
            {
                SpriteSystem.LayerSetDirOffset((uid, args.Sprite), slot.Arrow, direction.ToOffset());
            }
        }
    }
}
