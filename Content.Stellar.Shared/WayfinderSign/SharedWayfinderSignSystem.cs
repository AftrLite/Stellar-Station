// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.WayfinderSign;

public sealed class SharedWayfinderSignSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WayfinderSignComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<WayfinderSignComponent, EntRemovedFromContainerMessage>(OnRemove);
        SubscribeLocalEvent<WayfinderSignComponent, LockToggledEvent>(OnLockToggle);

        SubscribeLocalEvent<WayfinderLabelComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
    }

    private void OnInsert(Entity<WayfinderSignComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        UpdateSignAppearance(ent, args.Container.ID, true);
    }

    private void OnRemove(Entity<WayfinderSignComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateSignAppearance(ent, args.Container.ID, false);
    }

    private void OnLockToggle(Entity<WayfinderSignComponent> ent, ref LockToggledEvent args)
    {
        _itemSlots.SetLock(ent, ent.Comp.Slot1ID, args.Locked);
        _itemSlots.SetLock(ent, ent.Comp.Slot2ID, args.Locked);
        _itemSlots.SetLock(ent, ent.Comp.Slot3ID, args.Locked);
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
        ent.Comp.LabelDirection++;
        if (ent.Comp.LabelDirection > 3)
            ent.Comp.LabelDirection = 0;

        _appearance.SetData(ent, WayfinderLabelVisuals.Label, ent.Comp.LabelDirection);
        Dirty(ent);
    }

    private void UpdateSignAppearance(Entity<WayfinderSignComponent> ent, string slotName, bool enabled)
    {
        var currentLabel = WayfinderSignLayers.Slot1;

        if (slotName == ent.Comp.Slot2ID)
            currentLabel = WayfinderSignLayers.Slot2;
        else if (slotName == ent.Comp.Slot3ID)
            currentLabel = WayfinderSignLayers.Slot3;

        _appearance.SetData(ent, currentLabel, enabled);
    }
}
