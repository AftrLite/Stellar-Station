// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Interaction;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;

namespace Content.Stellar.Client.Science;

public sealed class StellarScienceAbeSystem : SharedStellarScienceAbeSystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAbeComponent, ActivateInWorldEvent>(OnApeActivateInWorld);
        SubscribeLocalEvent<StellarAbeComponent, InteractHandEvent>(OnApeInteractHand);
    }

    private void OnApeActivateInWorld(Entity<StellarAbeComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.Handled)
            return;

        if (DoAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        UiSystem.OpenUi(ent.Owner, StellarAbeRadialKey.Key, args.User, true);
    }

    private void OnApeInteractHand(Entity<StellarAbeComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (DoAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        UiSystem.OpenUi(ent.Owner, StellarAbeRadialKey.Key, args.User, true);
    }
}
