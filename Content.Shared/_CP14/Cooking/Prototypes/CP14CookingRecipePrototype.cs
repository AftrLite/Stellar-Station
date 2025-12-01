// SPDX-FileCopyrightText: 2025 TheShuEd
// SPDX-FileCopyrightText: 2025 AftrLite (Stellar Station Changes Only)
//
// SPDX-License-Identifier: MIT

/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Cooking.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CP14.Cooking.Prototypes;

[Prototype("CP14CookingRecipe")]
public sealed class CP14CookingRecipePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// List of conditions that must be met in the set of ingredients for a dish
    /// </summary>
    [DataField]
    public List<CP14CookingCraftRequirement> Requirements = new();

    /// <summary>
    /// Reagents cannot store all the necessary information about food, so along with the reagents for all the ingredients,
    /// in this block we add the appearance of the dish, descriptions, and so on.
    /// </summary>
    [DataField]
    public CP14FoodData FoodData = new();

    [DataField(required: true)]
    public ProtoId<CP14FoodTypePrototype> FoodType;

    [DataField]
    public TimeSpan CookingTime = TimeSpan.FromSeconds(20f);

    // BEGIN - STELLAR STATION CHANGES (AftrLite)
    [DataField]
    public TimeSpan RandTimeOffsetMin = TimeSpan.FromSeconds(0f);
    [DataField]
    public TimeSpan RandTimeOffsetMax = TimeSpan.FromSeconds(3f);

    [DataField]
    public HashSet<EntProtoId>? SpecificResults;
    // END - STELLAR STATION CHANGES (AftrLite)
}
