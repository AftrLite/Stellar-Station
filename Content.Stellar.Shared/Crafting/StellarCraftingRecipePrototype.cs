// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Crafting;

/// <summary>
/// An influence that can be purchased from the monument
/// </summary>
[Prototype]
public sealed partial class StellarCraftingRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("ingredients")]
    private Dictionary<string, int> _ingredients = new();

    [DataField(required: true)]
    public EntProtoId Output;

    [DataField]
    public TimeSpan CraftingTime =  TimeSpan.FromSeconds(5);

    public IReadOnlyDictionary<string, int> IngredientList => _ingredients;
}
