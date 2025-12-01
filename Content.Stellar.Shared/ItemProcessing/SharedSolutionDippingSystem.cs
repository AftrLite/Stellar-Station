// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Diagnostics.CodeAnalysis;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Stellar.Shared.ItemProcessing.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.ItemProcessing;

public sealed class SharedSolutionDippingSstem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DippableItemComponent, ItemDippingDoAfter>(OnDipped);
        SubscribeLocalEvent<DippableSolutionComponent, InteractUsingEvent>(OnInteractUsing);
    }

    /// <summary>
    /// Accessor that matches the format of SharedSolutionContainerSystem.Capabilities.
    /// </summary>
    public bool TryGetDippableSolution(Entity<DippableSolutionComponent?, SolutionContainerManagerComponent?> entity, [NotNullWhen(true)] out Entity<SolutionComponent>? soln, [NotNullWhen(true)] out Solution? solution)
    {
        if (!Resolve(entity, ref entity.Comp1, logMissing: false))
        {
            (soln, solution) = (default!, null);
            return false;
        }

        return _solution.TryGetSolution((entity.Owner, entity.Comp2), entity.Comp1.Solution, out soln, out solution);
    }

    private void OnInteractUsing(Entity<DippableSolutionComponent> entity, ref InteractUsingEvent args)
    {
        if (!TryComp<DippableItemComponent>(args.Used, out var dipped))
            return;
        TryDipping((args.Used, dipped), args);
        args.Handled = true;
    }

    private void TryDipping(Entity<DippableItemComponent> ent, InteractUsingEvent args)
    {
        if (!TryGetDippableSolution(args.Target, out var soln, out var solution))
            return;

        foreach (var reagent in solution.GetReagentPrototypes(_proto))
        {
            if (ent.Comp.ReagentAndResult.ContainsKey(reagent.Key) && reagent.Value.Value >= ent.Comp.NeededVolume)
            {
                ent.Comp.ReagentAndResult.TryGetValue(reagent.Key, out var dippingResult);
                var doAfterArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.DippingTime, new ItemDippingDoAfter(solution, dippingResult, reagent.Key.ID), ent, args.Target)
                {
                    NeedHand = true,
                    BreakOnMove = true,
                    BreakOnDropItem = true,
                    BreakOnHandChange = true,
                    BreakOnWeightlessMove = true,
                    RequireCanInteract = true,
                    DistanceThreshold = 1f,
                };
                ent.Comp.CurrentSolution = soln.Value;
                _doAfter.TryStartDoAfter(doAfterArgs);
                continue;
            }
        }
    }

    private void OnDipped(Entity<DippableItemComponent> ent, ref ItemDippingDoAfter args)
    {
        if (args.Target is null || args.Handled || args.Cancelled)
            return;

        args.Solution.RemoveReagent(args.Reagent, ent.Comp.NeededVolume);
        _solution.UpdateChemicals(ent.Comp.CurrentSolution);

        PredictedQueueDel(ent);

        var dipped = PredictedSpawnAtPosition(args.Result, args.User.ToCoordinates());
        _hands.TryPickupAnyHand(args.User, dipped);
    }
}

[Serializable, NetSerializable]
public sealed partial class ItemDippingDoAfter : DoAfterEvent
{
    [DataField]
    public Solution Solution;

    [DataField]
    public EntProtoId Result;

    [DataField]
    public string Reagent;

    public ItemDippingDoAfter(Solution solution, EntProtoId result, string reagent)
    {
        Solution = solution;
        Result = result;
        Reagent = reagent;
    }

    public override DoAfterEvent Clone() => this;
}
