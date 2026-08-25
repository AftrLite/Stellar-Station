// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science;

public sealed class SharedStellarScienceCapsuleSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popUp = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarContainmentCapsuleComponent, AfterInteractEvent>(OnAfterInteract);

        SubscribeLocalEvent<StellarContainmentCapsuleComponent, StellarCapsuleDoAfter>(OnCapsuleDoAfter);
        SubscribeLocalEvent<StellarContainmentCapsuleComponent, ExaminedEvent>(OnExamined);
    }

    private void OnAfterInteract(Entity<StellarContainmentCapsuleComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || !TryComp<StellarAnomalyComponent>(args.Target, out var anomComp) || !anomComp.Stable)
            return;

        if (ent.Comp.Full)
        {
            _popUp.PopupClient("This capsule is full!", ent, args.User, PopupType.MediumCaution); // TODO: Localization
            return;
        }

        var doArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.ContainmentTime, new StellarCapsuleDoAfter(), ent, args.Target, ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            BreakOnWeightlessMove = true,
            BreakOnHandChange = true,
            BreakOnDropItem = true,
        };

        args.Handled = true;
        _doAfter.TryStartDoAfter(doArgs);
    }

    private void OnCapsuleDoAfter(Entity<StellarContainmentCapsuleComponent> ent, ref StellarCapsuleDoAfter args)
    {
        if (args.Cancelled || args.Handled || !TryComp<StellarAnomalyComponent>(args.Target, out var anomComp))
            return;

        if (anomComp.IntegrityPipsValue is not { } pips || pips < 1)
            return;

        ent.Comp.Full = true;
        ent.Comp.StoredEnergy = pips;
        anomComp.IntegrityPipsValue = 0;
        _appearance.SetData(ent, StellarCapsuleVisuals.Visuals, 2);

        var ev = new StellarAnomalyDecrementEvent();
        RaiseLocalEvent(args.Target.Value, ref ev);
        Dirty(ent);
    }

    private void OnExamined(Entity<StellarContainmentCapsuleComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("containment-capsule-info"));
        if (ent.Comp.Full)
            args.PushMarkup(Loc.GetString("containment-capsule-full"));
        else
            args.PushMarkup(Loc.GetString("containment-capsule-empty"));
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarCapsuleDoAfter : SimpleDoAfterEvent;
