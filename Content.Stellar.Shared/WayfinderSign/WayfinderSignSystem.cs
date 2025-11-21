// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.WayfinderSign;

public sealed class WayfinderSignSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WayfinderSignComponent, ComponentInit>(OnComponentInitSign);
        SubscribeLocalEvent<WayfinderSignComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<WayfinderSignComponent, EntRemovedFromContainerMessage>(OnRemove);

        SubscribeLocalEvent<WayfinderLabelComponent, ComponentInit>(OnComponentInitLabel);
        SubscribeLocalEvent<WayfinderLabelComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
    }

    private void OnComponentInitSign(Entity<WayfinderSignComponent> ent, ref ComponentInit args)
    {
        _appearance.SetData(ent, WayfinderSignLayers.Slots, new WayfinderSignSlotsAppearance(ent.Comp.Slots));
    }

    private void OnComponentInitLabel(Entity<WayfinderLabelComponent> ent, ref ComponentInit args)
    {
        _appearance.SetData(ent, WayfinderLabelVisuals.Base, ent.Comp.BaseSprite);
        _appearance.SetData(ent, WayfinderLabelVisuals.Arrow, ent.Comp.ArrowSprite);
        _appearance.SetData(ent, WayfinderLabelVisuals.ArrowRotation, ent.Comp.ArrowRotation);
    }

    private void OnInsert(Entity<WayfinderSignComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateSignAppearance(ent.AsNullable(), args.Container.ID, args.Entity, true);
    }

    private void OnRemove(Entity<WayfinderSignComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateSignAppearance(ent.AsNullable(), args.Container.ID, args.Entity, false);
    }

    private void AddAlternativeVerbs(Entity<WayfinderLabelComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/rotate_cw.svg.192dpi.png")),
            Text = Loc.GetString("rotate-verb-get-data-text"),
            Priority = 1,
            CloseMenu = false,
            Act = () => RotateLabel(ent)
        });
    }

    public void RotateLabel(Entity<WayfinderLabelComponent> ent)
    {
        ent.Comp.ArrowRotation = (Direction)(((int)ent.Comp.ArrowRotation + 6) % 8);
        Dirty(ent);

        _appearance.SetData(ent, WayfinderLabelVisuals.ArrowRotation, ent.Comp.ArrowRotation);
        if (_container.TryGetContainingContainer(ent.Owner, out var container))
        {
            UpdateSignAppearance(container.Owner, container.ID, ent.AsNullable(), true);
        }
    }

    private void UpdateSignAppearance(Entity<WayfinderSignComponent?> ent, string containerID, Entity<WayfinderLabelComponent?> label, bool enabled)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!ent.Comp.Slots.TryGetValue(containerID, out var slot))
            return;

        if (!Resolve(label, ref label.Comp, false) || !enabled)
        {
            _appearance.RemoveData(ent, slot.Base);
            _appearance.RemoveData(ent, slot.Arrow);
            _appearance.RemoveData(ent, slot.Direction);
            return;
        }

        _appearance.SetData(ent, slot.Base, label.Comp.BaseSprite);
        _appearance.SetData(ent, slot.Arrow, label.Comp.ArrowSprite);
        _appearance.SetData(ent, slot.Direction, label.Comp.ArrowRotation);
    }
}
