// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.Crafting;
using Robust.Client.UserInterface;
using JetBrains.Annotations;

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

        _menu.OnRecipeButtonPressed += protoId => SendMessage(new RecipeSelectMessage(protoId));
        _menu.OnCancelButtonPressed += () => SendMessage(new RecipeCancelMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState bui)
    {
        base.UpdateState(bui);
        if (bui is not StellarCraftingBuiState state || _menu == null)
            return;

        _menu.UpdateRecipe(state.Owner);
    }
}
