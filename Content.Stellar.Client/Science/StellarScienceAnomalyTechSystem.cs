// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Interaction;
using Content.Stellar.Shared.Science;

namespace Content.Stellar.Client.Science;

public sealed class StellarScienceAnomalyTechSystem : SharedStellarScienceAnomalyTechSystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyAbeComponent, ActivateInWorldEvent>(OnApeActivateInWorld);
        SubscribeLocalEvent<StellarAnomalyAbeComponent, InteractHandEvent>(OnApeInteractHand);
    }

    private void OnApeActivateInWorld(Entity<StellarAnomalyAbeComponent> ent, ref ActivateInWorldEvent args)
    {
        if (!args.Complex || args.Handled)
            return;

        if (DoAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        UiSystem.OpenUi(ent.Owner, StellarAbeRadialKey.Key, args.User, true);
    }

    private void OnApeInteractHand(Entity<StellarAnomalyAbeComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (DoAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        UiSystem.OpenUi(ent.Owner, StellarAbeRadialKey.Key, args.User, true);
    }
}
