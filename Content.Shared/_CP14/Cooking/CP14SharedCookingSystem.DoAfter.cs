// SPDX-FileCopyrightText: 2025 TheShuEd
// SPDX-FileCopyrightText: 2025 AftrLite (Stellar Station Changes Only)
//
// SPDX-License-Identifier: MIT

/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CP14.Cooking.Components;
using Content.Shared._CP14.Cooking.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Power;
using Content.Shared.Temperature;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CP14.Cooking;

public abstract partial class CP14SharedCookingSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    private void InitDoAfter()
    {
        SubscribeLocalEvent<CP14FoodCookerComponent, OnTemperatureChangeEvent>(OnTemperatureChange);
        SubscribeLocalEvent<CP14FoodCookerComponent, EntParentChangedMessage>(OnParentChanged);

        SubscribeLocalEvent<CP14FoodCookerComponent, CP14CookingDoAfter>(OnCookFinished);
        SubscribeLocalEvent<CP14FoodCookerComponent, CP14BurningDoAfter>(OnCookBurned);

        SubscribeLocalEvent<CP14FoodCookerComponent, PowerChangedEvent>(OnPowerChanged); // STELLAR
    }
    // BEGIN - STELLAR STATION CHANGES (AftrLite)
    private void UpdateDoAfter(float frameTime)
    {
        var query = EntityQueryEnumerator<CP14FoodCookerComponent>();
        while (query.MoveNext(out var uid, out var cooker))
        {
            switch (cooker.CookMethod)
            {
                case CookingMethod.Temperature:
                    if (_timing.CurTime > cooker.LastHeatingTime + cooker.HeatingFrequencyRequired && _doAfter.IsRunning(cooker.DoAfterId))
                        _doAfter.Cancel(cooker.DoAfterId);
                    break;

                case CookingMethod.Power:
                    if (!cooker.Powered && _doAfter.IsRunning(cooker.DoAfterId))
                        _doAfter.Cancel(cooker.DoAfterId);
                    break;
                case CookingMethod.Nested:
                    if (!cooker.NestedCooking && _doAfter.IsRunning(cooker.DoAfterId))
                        _doAfter.Cancel(cooker.DoAfterId);
                    break;
            }
        }
    }
    // END - STELLAR STATION CHANGES (AftrLite)

    protected virtual void OnCookBurned(Entity<CP14FoodCookerComponent> ent, ref CP14BurningDoAfter args)
    {
        StopCooking(ent);

        if (args.Cancelled || args.Handled)
            return;

        BurntFood(ent);

        args.Handled = true;
    }

    protected virtual void OnCookFinished(Entity<CP14FoodCookerComponent> ent, ref CP14CookingDoAfter args)
    {
        StopCooking(ent);

        // BEGIN - STELLAR STATION CHANGES (AftrLite)
        if (args.Cancelled || args.Handled || !_proto.TryIndex(args.Recipe, out var indexedRecipe))
            return;

        if (!TryComp<CP14FoodHolderComponent>(ent, out var holder))
            return;

        if (indexedRecipe.SpecificResults is not null)
        {
            CreateSpecificResults(ent, indexedRecipe);
        }
        else
        {
            CreateFoodData(ent, indexedRecipe);
            UpdateFoodDataVisuals((ent, holder), ent.Comp.RenameCooker);
        }

        if (ent.Comp.CanBurn && ent.Comp.CookMethod != CookingMethod.Temperature)
        {
            StartBurning(ent); // The cooker says it can burn despite the method not being temperature? Burn it anyway!
        }
        // END - STELLAR STATION CHANGES (AftrLite)

        args.Handled = true;
    }

    private void StartCooking(Entity<CP14FoodCookerComponent> ent, CP14CookingRecipePrototype recipe)
    {
        if (_doAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        // BEGIN - STELLAR STATION CHANGES (AftrLite)
        if (ent.Comp.CookMethod == CookingMethod.Nested)
        {
            var cookTimeOffset = _random.Next(recipe.RandTimeOffsetMin, recipe.RandTimeOffsetMax);
            var xform = Transform(ent);
            _container.TryGetOuterContainer(ent, xform, out var container);

            if (container == null)
                return;

            var doAfterArgs = new DoAfterArgs(EntityManager, container.Owner, recipe.CookingTime + cookTimeOffset, new CP14CookingDoAfter(recipe.ID), ent, ent)
            {
                NeedHand = false,
                BreakOnWeightlessMove = false,
                RequireCanInteract = false,
                BlockDuplicate = false,
            };

            _ambientSound.SetAmbience(container.Owner, true);
            _appearance.SetData(container.Owner, CP14CookingVisuals.Cooking, true);
            _appearance.SetData(container.Owner, CP14CookingVisuals.Active, true);
            _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
            ent.Comp.DoAfterId = doAfterId;
        }
        else // NOT NESTED
        {
            var cookTimeOffset = _random.Next(recipe.RandTimeOffsetMin, recipe.RandTimeOffsetMax);
            var doAfterArgs = new DoAfterArgs(EntityManager, ent, recipe.CookingTime + cookTimeOffset, new CP14CookingDoAfter(recipe.ID), ent)
            {
                NeedHand = false,
                BreakOnWeightlessMove = false,
                RequireCanInteract = false,
            };

            _ambientSound.SetAmbience(ent, true);
            _appearance.SetData(ent, CP14CookingVisuals.Cooking, true);
            _appearance.SetData(ent, CP14CookingVisuals.Active, true);
            _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
            ent.Comp.DoAfterId = doAfterId;
        }
        // END - STELLAR STATION CHANGES (AftrLite)
    }

    private void StartBurning(Entity<CP14FoodCookerComponent> ent)
    {
        if (_doAfter.IsRunning(ent.Comp.DoAfterId))
            return;

        // BEGIN - STELLAR STATION CHANGES (AftrLite)
        if (ent.Comp.CookMethod == CookingMethod.Nested)
        {
            var xform = Transform(ent);
            _container.TryGetOuterContainer(ent, xform, out var container);

            if (container == null)
                return;

            _appearance.SetData(container.Owner, CP14CookingVisuals.Burning, true);
            _appearance.SetData(container.Owner, CP14CookingVisuals.Active, true);

            var doAfterArgs = new DoAfterArgs(EntityManager, container.Owner, 20, new CP14BurningDoAfter(), ent, ent)
            {
                NeedHand = false,
                BreakOnWeightlessMove = false,
                RequireCanInteract = false,
                BlockDuplicate = false,
            };

            _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
            _ambientSound.SetAmbience(container.Owner, true);
            ent.Comp.DoAfterId = doAfterId;
        }
        else // NOT NESTED
        {
            _appearance.SetData(ent, CP14CookingVisuals.Burning, true);
            _appearance.SetData(ent, CP14CookingVisuals.Active, true);

            var doAfterArgs = new DoAfterArgs(EntityManager, ent, 20, new CP14BurningDoAfter(), ent)
            {
                NeedHand = false,
                BreakOnWeightlessMove = false,
                RequireCanInteract = false,
            };

            _doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId);
            _ambientSound.SetAmbience(ent, true);
            ent.Comp.DoAfterId = doAfterId;
        }
        // END - STELLAR STATION CHANGES (AftrLite)
    }

    protected void StopCooking(Entity<CP14FoodCookerComponent> ent, Entity<CP14CookerRelayComponent>? relay = null)
    {
        if (_doAfter.IsRunning(ent.Comp.DoAfterId))
            _doAfter.Cancel(ent.Comp.DoAfterId);

        // BEGIN - STELLAR STATION CHANGES (AftrLite)
        if (ent.Comp.CookMethod == CookingMethod.Nested)
        {
            if (relay == null)
                return;

            _appearance.SetData(relay.Value, CP14CookingVisuals.Cooking, false);
            _appearance.SetData(relay.Value, CP14CookingVisuals.Burning, false);
            _appearance.SetData(relay.Value, CP14CookingVisuals.Active, false);
            _ambientSound.SetAmbience(relay.Value, false);
        }
        else // NOT NESTED
        {
            _appearance.SetData(ent, CP14CookingVisuals.Cooking, false);
            _appearance.SetData(ent, CP14CookingVisuals.Burning, false);
            _appearance.SetData(ent, CP14CookingVisuals.Active, false);
            _ambientSound.SetAmbience(ent, false);
        }
        // END - STELLAR STATION CHANGES (AftrLite)
    }

    // BEGIN - STELLAR STATION CHANGES (AftrLite)
    private void OnPowerChanged(Entity<CP14FoodCookerComponent> ent, ref PowerChangedEvent args)
    {
        if (ent.Comp.CookMethod == CookingMethod.Power)
            ent.Comp.Powered = args.Powered;
        else return;

        if (!TryComp<CP14FoodHolderComponent>(ent, out var holder))
            return;

        if (args.Powered)
        {
            if (!_doAfter.IsRunning(ent.Comp.DoAfterId) && holder.FoodData is null)
            {
                var recipe = GetRecipe(ent);
                if (recipe is not null)
                    StartCooking(ent, recipe);
            }
            else
            {
                if (ent.Comp.CanBurn)
                    StartBurning(ent);
            }
        }
        else
        {
            StopCooking(ent);
        }
    }
    // END - STELLAR STATION CHANGES (AftrLite)

    private void OnTemperatureChange(Entity<CP14FoodCookerComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (ent.Comp.CookMethod != CookingMethod.Temperature || !_container.TryGetContainer(ent, ent.Comp.ContainerId, out var container))
            return;

        if (!TryComp<CP14FoodHolderComponent>(ent, out var holder))
            return;

        if (container.ContainedEntities.Count <= 0 && holder.FoodData is null)
        {
            StopCooking(ent);
            return;
        }

        if (args.TemperatureDelta > 0)
        {
            ent.Comp.LastHeatingTime = _timing.CurTime;
            DirtyField(ent.Owner, ent.Comp, nameof(CP14FoodCookerComponent.LastHeatingTime));

            if (!_doAfter.IsRunning(ent.Comp.DoAfterId) && holder.FoodData is null)
            {
                var recipe = GetRecipe(ent);
                if (recipe is not null)
                    StartCooking(ent, recipe);
            }
            else
            {
                StartBurning(ent);
            }
        }
        else
        {
            StopCooking(ent);
        }
    }

    private void OnParentChanged(Entity<CP14FoodCookerComponent> ent, ref EntParentChangedMessage args)
    {
        StopCooking(ent);
    }
}

[Serializable, NetSerializable]
public sealed partial class CP14CookingDoAfter : DoAfterEvent
{
    [DataField]
    public ProtoId<CP14CookingRecipePrototype> Recipe;

    public CP14CookingDoAfter(ProtoId<CP14CookingRecipePrototype> recipe)
    {
        Recipe = recipe;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class CP14BurningDoAfter : SimpleDoAfterEvent;
