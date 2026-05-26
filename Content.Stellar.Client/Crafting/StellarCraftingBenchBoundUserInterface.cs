// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Client.UserInterface.Controls;
using Content.Stellar.Server.CosmicCult.Components;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Robust.Shared.Collections;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.Crafting;

/// <summary>
/// The radial menu used by Stellar A.P.Es.
/// </summary>
[UsedImplicitly]
public sealed class StellarCraftingBenchBoundUserInterface : BoundUserInterface
{
    private StellarCraftingMenu? _menu;

    public StellarCraftingBenchBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<StellarCraftingMenu>();
        _menu.SetEntity(Owner);
    }
}
