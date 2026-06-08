// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;

namespace Content.Stellar.Client.Science;

public sealed class StellarSciencePortablesSystem : SharedStellarSciencePortablesSystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSciencePortablesComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<StellarSciencePortablesComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnActivateInWorld(Entity<StellarSciencePortablesComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.Handled)
            return;

        HandleUI(ent, args.User);
    }

    private void OnInteractHand(Entity<StellarSciencePortablesComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        HandleUI(ent, args.User);
    }

    private void HandleUI(Entity<StellarSciencePortablesComponent> ent, EntityUid user)
    {
        if (DoAfter.IsRunning(ent.Comp.DoAfterId) || UiSystem.IsUiOpen(ent.Owner, StellarPortablesRadialKey.Key))
            return;

        UiSystem.OpenUi(ent.Owner, StellarPortablesRadialKey.Key, user, true);
    }
}
