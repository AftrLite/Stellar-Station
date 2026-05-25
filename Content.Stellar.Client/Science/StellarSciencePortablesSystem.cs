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

        SubscribeLocalEvent<StellarSciencePortablesComponent, ActivateInWorldEvent>(OnApeActivateInWorld);
        SubscribeLocalEvent<StellarSciencePortablesComponent, InteractHandEvent>(OnApeInteractHand);
    }

    private void OnApeActivateInWorld(Entity<StellarSciencePortablesComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.Handled)
            return;

        if (DoAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        UiSystem.OpenUi(ent.Owner, StellarPortablesRadialKey.Key, args.User, true);
    }

    private void OnApeInteractHand(Entity<StellarSciencePortablesComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (DoAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        UiSystem.OpenUi(ent.Owner, StellarPortablesRadialKey.Key, args.User, true);
    }
}
