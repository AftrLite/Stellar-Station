// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Linq;
using System.Numerics;
using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Crafting;

public abstract class SharedStellarCraftingSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem PopUp = default!;
    [Dependency] protected readonly SharedPowerReceiverSystem Power = default!;

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    private const string ContainerNameInput = "crafting-input";
    private const string ContainerNameOutput = "crafting-output";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarCraftingBenchComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<StellarCraftingBenchComponent, StellarCraftingBenchDoAfter>(OnCraftingBenchDoAfter);

        SubscribeLocalEvent<StellarCraftingBenchComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<StellarCraftingBenchComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<StellarCraftingBenchComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<StellarCraftingBenchComponent, RecipeSelectMessage>(OnRecipeSelect);
        SubscribeLocalEvent<StellarCraftingBenchComponent, RecipeCancelMessage>(OnRecipeCancel);
    }

    private void OnPowerChanged(Entity<StellarCraftingBenchComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered && ent.Comp.ReadyToCraft)
        {
            StartCrafting(ent);
            return;
        }

        if (args.Powered && ent.Comp.CraftingComplete)
        {
            _appearance.SetData(ent, StellarCraftingBenchVisuals.Visuals, StellarCraftingBenchState.AwaitingPickup);
            return;
        }

        if (args.Powered)
        {
            _appearance.SetData(ent, StellarCraftingBenchVisuals.Visuals, StellarCraftingBenchState.Idle);
            return;
        }

        if (!args.Powered)
        {
            _appearance.SetData(ent, StellarCraftingBenchVisuals.Visuals, StellarCraftingBenchState.Unpowered);
            _doAfter.Cancel(ent.Comp.DoAfterId);
            _ambient.SetAmbience(ent, false);
        }
    }

    private void OnCraftingBenchDoAfter(Entity<StellarCraftingBenchComponent> ent, ref StellarCraftingBenchDoAfter args)
    {
        if (args.Handled || args.Cancelled)
            return;

        PredictedSpawnInContainerOrDrop(ent.Comp.Output, ent, ContainerNameOutput);
        RaiseNetworkEvent(new StellarCraftingVisualsEvent(GetNetEntity(ent)));
        _appearance.SetData(ent, StellarCraftingBenchVisuals.Visuals, StellarCraftingBenchState.AwaitingPickup);
        _audio.PlayPredicted(ent.Comp.FinishSound, ent, args.User);
        _ambient.SetAmbience(ent, false);

        ent.Comp.CraftingComplete = true;
        ent.Comp.ReadyToCraft = false;
        Dirty(ent);
    }

    private void StartCrafting(Entity<StellarCraftingBenchComponent> ent)
    {
        _ui.CloseUis(ent.Owner);
        _appearance.SetData(ent, StellarCraftingBenchVisuals.Visuals, StellarCraftingBenchState.Crafting);

        foreach (var emptiedEnt in _container.EmptyContainer(ent.Comp.InputStorage))
        {
            PredictedQueueDel(emptiedEnt);
        }

        var doArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.CraftTime, new StellarCraftingBenchDoAfter(), ent, ent)
        {
            NeedHand = false,
            BreakOnWeightlessMove = false,
            BreakOnMove = false,
            BreakOnDamage = false,
            RequireCanInteract = false,
        };
        _doAfter.TryStartDoAfter(doArgs, out var doAfter);

        ent.Comp.DoAfterId = doAfter;
        ent.Comp.SelectedRecipe = null;
        ent.Comp.ReadyToCraft = true;
        ent.Comp.IngredientReqs.Clear();
        Dirty(ent);
    }

    private void OnInteractUsing(Entity<StellarCraftingBenchComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !_container.CanInsert(args.Used, ent.Comp.InputStorage) || !Power.IsPowered(ent.Owner))
            return;

        args.Handled = HandleIngredientInput(ent, args.Used, args.User);
        if (ent.Comp.IngredientReqs.Count == 0 && ent.Comp.SelectedRecipe != null)
        {
            _audio.PlayPredicted(ent.Comp.StartSound, ent, args.User);
            _ambient.SetAmbience(ent, true);
            StartCrafting(ent);
            Dirty(ent);
        }
    }

    private bool HandleIngredientInput(Entity<StellarCraftingBenchComponent> ent, EntityUid used, EntityUid user)
    {
        var toSubtract = 1;
        var metaData = Comp<MetaDataComponent>(used);

        if (ent.Comp.SelectedRecipe == null)
            return false;

        if (TryComp<StackComponent>(used, out var stackComp) && ent.Comp.IngredientReqs.ContainsKey(_proto.Index(stackComp.StackTypeId).Spawn))
        {
            var stackId = _proto.Index(stackComp.StackTypeId).Spawn;
            toSubtract = Math.Clamp(ent.Comp.IngredientReqs[stackId.Id], 0, stackComp.Count);

            _stack.ReduceCount((used, stackComp), toSubtract);
            ent.Comp.IngredientReqs[stackId.Id] -= toSubtract;
            if (ent.Comp.IngredientReqs[stackId.Id] <= 0)
                ent.Comp.IngredientReqs.Remove(stackId.Id);

            var inserted = PredictedSpawnInContainerOrDrop(stackId.Id, ent, ContainerNameInput);
            _stack.SetCount((inserted, null), toSubtract);
        }

        else if (metaData.EntityPrototype != null && ent.Comp.IngredientReqs.ContainsKey(metaData.EntityPrototype.ID))
        {
            ent.Comp.IngredientReqs[metaData.EntityPrototype.ID] -= toSubtract;
            if (ent.Comp.IngredientReqs[metaData.EntityPrototype.ID] <= 0)
                ent.Comp.IngredientReqs.Remove(metaData.EntityPrototype.ID);

            _hands.TryDropIntoContainer(user, used, ent.Comp.InputStorage);
        }
        else
            return false;

        _audio.PlayPredicted(ent.Comp.InsertSound, ent, user);
        _ui.SetUiState(ent.Owner, StellarCraftingKey.Key, new StellarCraftingBuiState(GetNetEntity(ent)));
        Dirty(ent);
        return true;
    }

    protected virtual void OnInteractHand(Entity<StellarCraftingBenchComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.OutputStorage.Count > 0)
        {
            _appearance.SetData(ent, StellarCraftingBenchVisuals.Visuals, StellarCraftingBenchState.Idle);
            _hands.TryPickupAnyHand(args.User, _container.EmptyContainer(ent.Comp.OutputStorage).First());
            _audio.PlayPredicted(ent.Comp.EjectSound, ent, args.User);
            ent.Comp.CraftingComplete = false;
            args.Handled = true;
            Dirty(ent);
        }
    }

    private void OnComponentInit(Entity<StellarCraftingBenchComponent> ent, ref ComponentInit args)
    {
        ent.Comp.InputStorage = _container.EnsureContainer<Container>(ent.Owner, ContainerNameInput);
        ent.Comp.OutputStorage = _container.EnsureContainer<Container>(ent.Owner, ContainerNameOutput);
    }

    private void OnRecipeSelect(Entity<StellarCraftingBenchComponent> ent, ref RecipeSelectMessage args)
    {
        if (ent.Comp.SelectedRecipe == args.RecipeId || !_proto.TryIndex(args.RecipeId, out var proto))
            return;

        ent.Comp.IngredientReqs.Clear();
        foreach (var ingredient in proto.IngredientList)
        {
            ent.Comp.IngredientReqs.Add(ingredient.Key, ingredient.Value);
        }

        ent.Comp.CraftTime = proto.CraftingTime;
        ent.Comp.SelectedRecipe = args.RecipeId;
        ent.Comp.Output = proto.Output;
        Dirty(ent);
    }

    private void OnRecipeCancel(Entity<StellarCraftingBenchComponent> ent, ref RecipeCancelMessage args)
    {
        var items = _container.EmptyContainer(ent.Comp.InputStorage);
        ent.Comp.IngredientReqs.Clear();
        ent.Comp.SelectedRecipe = null;
        Dirty(ent);

        foreach (var item in items)
        {
            _physics.ApplyLinearImpulse(item, new Vector2(_random.NextFloat(-5, +5), _random.NextFloat(-5, +5)) * 30);
            _physics.ApplyAngularImpulse(item, _random.NextFloat(-12, +12));
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarCraftingBenchDoAfter : SimpleDoAfterEvent;

[NetSerializable, Serializable]
public sealed class StellarCraftingBuiState : BoundUserInterfaceState
{
    public NetEntity Owner;

    public StellarCraftingBuiState(NetEntity owner)
    {
        Owner = owner;
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarCraftingVisualsEvent(NetEntity target) : EntityEventArgs
{
    public NetEntity Target = target;
}
