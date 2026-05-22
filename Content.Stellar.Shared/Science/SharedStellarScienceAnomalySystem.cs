// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ES.Camera;
using Content.Shared._ES.Core.Timer.Components;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Trigger.Systems;
using Content.Stellar.Shared._ES.Core.Timer;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Science;

public abstract class SharedStellarScienceAnomalySystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly ESEntityTimerSystem Timer = default!;
    [Dependency] protected readonly ESScreenshakeSystem Shake = default!;
    [Dependency] protected readonly SharedAudioSystem Audio = default!;

    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popUp = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<StellarAnomalyComponent, ExaminedEvent>(OnAnomalyExamined);

        SubscribeLocalEvent<StellarAnomalyComponent, StellarAnomalyPulseEvent>(OnPulseAnom);
        SubscribeLocalEvent<StellarAnomalyComponent, StellarAnomalyDecrementEvent>(OnDecrementAnom);
        SubscribeLocalEvent<StellarAnomalyComponent, StellarAnomalyStabilizeEvent>(OnStabilizeAnom);
        SubscribeLocalEvent<StellarAnomalyComponent, StellarAnomalyDestabilizeEvent>(OnDestabilizeAnom);
    }

    private void OnInteractUsing(Entity<StellarAnomalyComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !ent.Comp.Stable || !TryComp<StellarContainmentCapsuleComponent>(args.Used, out var capsuleComp))
            return;

        if (capsuleComp.Full)
        {
            _popUp.PopupClient("This capsule is full!", ent, args.User, PopupType.MediumCaution); // TODO: Localization
            return;
        }

        var doArgs = new DoAfterArgs(EntityManager, args.User, capsuleComp.ContainmentTime, new StellarCapsuleDoAfter(), args.Used, ent);
        _doAfter.TryStartDoAfter(doArgs);
    }

    private void OnAnomalyExamined(Entity<StellarAnomalyComponent> ent, ref ExaminedEvent args)
    {
        var baseText = ent.Comp.Stable ? "anomaly-examine-stable" : "anomaly-examine-unstable";
        var integrityText = "";
        switch (ent.Comp.IntegrityPipsValue)
        {
            case <=2:
                integrityText = "anomaly-examine-low";
                break;
            case <=4:
                integrityText = "anomaly-examine-mid";
                break;
            case 5:
                integrityText = "anomaly-examine-high";
                break;
            case >5:
                integrityText = "anomaly-examine-extra";
                break;
        }
        args.PushMarkup(Loc.GetString(baseText) + Loc.GetString(integrityText));
    }

    protected void MakeAnomalyPulse(Entity<StellarAnomalyComponent> ent)
    {
        var ev = new StellarAnomalyPulseEvent();
        Timer.SpawnMethodTimer(TimeSpan.FromSeconds(1.65), () => Shake.LerpedShake(ent, 0.66f, 0.75f, 0.0085f, 40f)); // Yucky timers! Used to sync up our audiovisual sauce.
        Timer.SpawnMethodTimer(TimeSpan.FromSeconds(1.65), () => PredictedSpawnAtPosition(ent.Comp.PulseVfx, Transform(ent).Coordinates));
        Timer.SpawnMethodTimer(TimeSpan.FromSeconds(1.65), () => RaiseLocalEvent(ent, ref ev));
        Audio.PlayPredicted(ent.Comp.SoundPulse, ent, ent);
    }

    protected string AnomalyCode(Entity<StellarAnomalyComponent> ent)
    {
        var code = "";
        for (var i = 0; i < ent.Comp.CodeLength; i++)
        {
            var c = Random.Next(1, 3 + 1);
            code += $"{c}";
        }
        return code;
    }

    #region Anomaly Events
    private void OnPulseAnom(Entity<StellarAnomalyComponent> ent, ref StellarAnomalyPulseEvent args)
    {
        if (Random.Prob(ent.Comp.TriggerChance))
            _trigger.Trigger(ent);
    }

    private void OnDecrementAnom(Entity<StellarAnomalyComponent> ent, ref StellarAnomalyDecrementEvent args)
    {
        var time = ent.Comp.Stable ? ent.Comp.PipTimeStable : Random.Next(ent.Comp.PipTimeMin, ent.Comp.PipTimeMax);
        ent.Comp.CurrentPipTimer = Timing.CurTime + time;
        ent.Comp.IntegrityPipsValue--;
        if (ent.Comp.IntegrityPipsValue <= 0)
            EnsureComp<ESTimedDespawnComponent>(ent);
    }

    private void OnStabilizeAnom(Entity<StellarAnomalyComponent> ent, ref StellarAnomalyStabilizeEvent args)
    {
        ent.Comp.CurrentPipTimer = Timing.CurTime + ent.Comp.PipTimeStable;
        ent.Comp.EnteredCode = string.Empty;
        ent.Comp.Stable = true;
        Dirty(ent);
    }

    private void OnDestabilizeAnom(Entity<StellarAnomalyComponent> ent, ref StellarAnomalyDestabilizeEvent args)
    {
        LocId text = "";
        if (ent.Comp.EnteredCode.Length > 0)
            text = "Stabilization scrambled!"; // TODO: Localization

        if (ent.Comp.Stable)
            text = "Destabilized!"; // TODO: Localization

        _popUp.PopupPredicted(text, ent, ent, PopupType.Large); // TODO: Localization | PopupPredicted(Loc.GetString("sensortower-popup-synccancelled"), uid, uid, PopupType.Large);

        ent.Comp.Stable = false;
        ent.Comp.EnteredCode = string.Empty;
        ent.Comp.AnomalyCode = AnomalyCode(ent);
        Dirty(ent);
    }
    #endregion
}

// I figured it'd be easier to handle anomalies through ByRefs than by making an API for them.
[ByRefEvent]
public readonly record struct StellarAnomalyPulseEvent;

[ByRefEvent]
public readonly record struct StellarAnomalyDecrementEvent;

[ByRefEvent]
public readonly record struct StellarAnomalyStabilizeEvent;

[ByRefEvent]
public readonly record struct StellarAnomalyDestabilizeEvent;

[Serializable, NetSerializable]
public sealed partial class StellarAnomalyReactionVisualsEvent : EntityEventArgs
{
    public NetEntity Target;

    public NetEntity Source;

    public string CodeCache;

    public StellarAnomalyReactionVisualsEvent(NetEntity target, NetEntity source, string codeCache = "")
    {
        Target = target;
        Source = source;
        CodeCache = codeCache;
    }
}
