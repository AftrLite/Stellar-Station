// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Crafting;


[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarCraftingBenchComponent : Component
{
    public Container OutputStorage = default!;

    public Container InputStorage = default!;

    public DoAfterId? DoAfterId = default!;

    [DataField, AutoNetworkedField] public ProtoId<StellarCraftingRecipePrototype>? SelectedRecipe;

    [DataField, AutoNetworkedField] public Dictionary<string, int> IngredientReqs = new();

    [DataField, AutoNetworkedField] public HashSet<ProtoId<StellarCraftingRecipePrototype>> AvailableRecipes = [];

    [DataField, AutoNetworkedField] public bool ReadyToCraft;

    [DataField, AutoNetworkedField] public bool CraftingComplete;

    [DataField, AutoNetworkedField] public TimeSpan CraftTime;

    [DataField, AutoNetworkedField] public EntProtoId Output;

    [DataField] public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");

    [DataField] public SoundSpecifier? EjectSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagOut/revolver_magout.ogg");

    [DataField] public SoundSpecifier? FinishSound = new SoundPathSpecifier("/Audio/_ST/Machines/crafting-complete.ogg");

    [DataField] public SoundSpecifier? StartSound = new SoundPathSpecifier("/Audio/_ST/Machines/crafting-started.ogg");
}

[Serializable, NetSerializable]
public enum StellarCraftingBenchVisuals
{
    Visuals,
}

[Serializable, NetSerializable]
public enum StellarCraftingBenchState
{
    Unpowered,
    Idle,
    Crafting,
    AwaitingPickup,
}

[Serializable, NetSerializable]
public enum StellarCraftingKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class RecipeSelectMessage(ProtoId<StellarCraftingRecipePrototype> recipeId) : BoundUserInterfaceMessage
{
    public ProtoId<StellarCraftingRecipePrototype> RecipeId = recipeId;
}

[Serializable, NetSerializable]
public sealed class RecipeCancelMessage : BoundUserInterfaceMessage;
