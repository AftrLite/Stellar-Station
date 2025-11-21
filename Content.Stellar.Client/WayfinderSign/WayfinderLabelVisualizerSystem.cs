// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.WayfinderSign;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.WayfinderSign;

public sealed class WayfinderLabelVisualizerSystem : VisualizerSystem<WayfinderLabelVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, WayfinderLabelVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (AppearanceSystem.TryGetData<SpriteSpecifier>(uid, WayfinderLabelVisuals.Base, out var @base, args.Component))
        {
            SpriteSystem.LayerSetSprite((uid, args.Sprite), WayfinderLabelVisuals.Base, @base);
        }
        if (AppearanceSystem.TryGetData<SpriteSpecifier>(uid, WayfinderLabelVisuals.Arrow, out var arrow, args.Component))
        {
            SpriteSystem.LayerSetSprite((uid, args.Sprite), WayfinderLabelVisuals.Arrow, arrow);
        }
        if (AppearanceSystem.TryGetData<Direction>(uid, WayfinderLabelVisuals.ArrowRotation, out var rotation, args.Component))
        {
            SpriteSystem.LayerSetDirOffset((uid, args.Sprite), WayfinderLabelVisuals.Arrow, rotation.ToOffset());
        }
    }
}
