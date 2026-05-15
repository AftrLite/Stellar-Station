// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening


using Content.Stellar.Server.HazardSectors;
using Content.Stellar.Shared.Science;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Server.Science;

public sealed class StellarScienceAnomalyTechSystem : SharedStellarScienceAnomalyTechSystem
{
    [Dependency] private readonly StellarGameRuleSystem<StellarShiftIntensityRuleComponent> _shiftIntensity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarRealityAnchorComponent, StellarAnchorAutoEnableDoAfter>(OnAnchorDoAfterEnable);
        SubscribeLocalEvent<StellarRealityAnchorComponent, StellarAnchorDisableDoAfter>(OnAnchorDoAfterDisable);
    }


    private void OnAnchorDoAfterEnable(Entity<StellarRealityAnchorComponent> ent, ref StellarAnchorAutoEnableDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        ent.Comp.Enabled = true;
    }

    private void OnAnchorDoAfterDisable(Entity<StellarRealityAnchorComponent> ent, ref StellarAnchorDisableDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (_shiftIntensity.GetShiftIntensity() is not { } intensity)
            return;

        var toSpawn = Random.Next(1, 3);
    }
}
