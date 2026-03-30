// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ES.Sparks;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Mobs.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Stellarator;

public abstract partial class SharedStellarEngineSystem : EntitySystem
{
    [Dependency] protected readonly ESSparksSystem Sparks = default!;
    [Dependency] protected readonly IPrototypeManager Proto = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly SharedPointLightSystem Lights = default!;

    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarEngineWireComponent, StepTriggerAttemptEvent>(OnWireStepAttempt);
        SubscribeLocalEvent<StellarEngineWireComponent, StepTriggeredOffEvent>(OnWireStepped);

        SubscribeLocalEvent<StellarFuelRodComponent, ExaminedEvent>(OnFuelExamined);
        SubscribeLocalEvent<StellarEngineTunerComponent, ExaminedEvent>(OnTunerExamined);

        SubscribeLocalEvent<StellarEngineCoreComponent, StellarEngineChaosEvent>(RelayCoreEvent);
        SubscribeLocalEvent<StellarEngineCoreComponent, StellarEngineStartupEvent>(RelayCoreEvent);
        SubscribeLocalEvent<StellarEngineCoreComponent, StellarEngineShutdownEvent>(RelayCoreEvent);
        SubscribeLocalEvent<StellarEngineCoreComponent, StellarEngineChargedEvent>(RelayCoreEvent);
        SubscribeLocalEvent<StellarEngineCoreComponent, StellarEngineUpdateEvent>(RelayCoreEvent);
        SubscribeLocalEvent<StellarEngineCoreComponent, StellarEngineEjectFuelEvent>(RelayCoreEvent);
        SubscribeLocalEvent<StellarEngineCoreComponent, StellarEngineGracePeriodEndEvent>(RelayCoreEvent);
    }

    private void OnFuelExamined(Entity<StellarFuelRodComponent> ent, ref ExaminedEvent args)
    {
        Proto.TryIndex(ent.Comp.ProvidedOvercharge, out var proto);
        if (proto == null)
            return;

        args.PushMarkup(Loc.GetString("overcharge-examine-text", ("name", Loc.GetString(proto.Name))));
    }

    private void OnTunerExamined(Entity<StellarEngineTunerComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.TunerBroken)
            args.PushMarkup(Loc.GetString("stellarator-tuner-broken-examine-text"));

        if (ent.Comp.TunerCurrentCalibration > ent.Comp.TunerCalibrationWindowGreen.Max)
            args.PushMarkup(Loc.GetString("stellarator-tuner-overcalibrated-examine-text"));

        switch (ent.Comp.State)
        {
            case TunerState.NotCalibrated:
                args.PushMarkup(Loc.GetString("stellarator-tuner-uncalibrated-examine-text"));
                break;
            case TunerState.NearCalibrated:
                args.PushMarkup(Loc.GetString("stellarator-tuner-near-calibrated-examine-text"));
                break;
            case TunerState.Calibrated:
                args.PushMarkup(Loc.GetString("stellarator-tuner-calibrated-examine-text"));
                break;
        }
    }

    protected void RelayCoreEvent<T>(Entity<StellarEngineCoreComponent> core, ref T args) where T : struct
    {
        var ev = new StellarEngineRelayedEvent<T>(core, args);
        foreach (var part in core.Comp.LinkedParts)
        {
            RaiseLocalEvent(part, ref ev);
        }
        args = ev.Args;
    }

    /*private void OnStartCollide(Entity<StellarEngineContactPadComponent> ent, ref StartCollideEvent args)
    {
        if (!_mobState.IsAlive(args.OtherEntity) || DoAfter.IsRunning(ent.Comp.ActiveDoAfter) || !ent.Comp.Enabled)
            return;

        ent.Comp.CurrentUser = args.OtherEntity;
        ent.Comp.CheckTime = Timing.CurTime + ent.Comp.StoredCheckTime;
        Dirty(ent);
    }*/

    /*private void OnEndCollide(Entity<StellarEngineContactPadComponent> ent, ref EndCollideEvent args)
    {
        if (ent.Comp.CurrentUser != null && args.OtherEntity != ent.Comp.CurrentUser)
            return;
        ent.Comp.CurrentUser = null;
        ent.Comp.Started = false;
        Dirty(ent);
    }*/
}

/// <summary>
/// Event wrapper for relayed events.
/// </summary>
[ByRefEvent]
public record struct StellarEngineRelayedEvent<TEvent>(Entity<StellarEngineCoreComponent> Core, TEvent Args);

[ByRefEvent]
public readonly record struct StellarEngineChaosEvent(float Intensity);

[ByRefEvent]
public readonly record struct StellarEngineStartupEvent(Entity<StellarFuelRodComponent> FuelRod);

[ByRefEvent]
public readonly record struct StellarEngineShutdownEvent(bool HasFuel);

[ByRefEvent]
public readonly record struct StellarEngineChargedEvent;

[ByRefEvent]
public readonly record struct StellarEngineEjectFuelEvent;

[ByRefEvent]
public readonly record struct StellarEngineGracePeriodEndEvent;

[ByRefEvent]
public record struct StellarEngineUpdateEvent(float TimeDelta, float LerpCalibration, int NearCalibrated, int Calibrated, int NotCalibrated);
