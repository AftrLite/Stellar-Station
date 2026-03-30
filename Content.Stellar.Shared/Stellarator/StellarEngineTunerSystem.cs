// SPDX-FileCopyrightText: 2026 AftrLite
// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ES.Sparks;
using Content.Shared.Audio;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Stellarator;

public sealed class StellarEngineTunerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ESSparksSystem _sparks = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarEngineTunerComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<StellarEngineTunerComponent, EndCollideEvent>(OnEndCollide);

        SubscribeLocalEvent<StellarEngineTunerComponent, StellarEngineTunerDoAfter>(OnTuningDoAfter);
        SubscribeLocalEvent<StellarEngineTunerComponent, WeldableAttemptEvent>(OnTunerAttemptWeld);
        SubscribeLocalEvent<StellarEngineTunerComponent, WeldableChangedEvent>(OnTunerWelded);

        SubscribeLocalEvent<StellarEngineTunerComponent, StellarEngineRelayedEvent<StellarEngineChaosEvent>>(OnChaosEvent);
        SubscribeLocalEvent<StellarEngineTunerComponent, StellarEngineRelayedEvent<StellarEngineStartupEvent>>(OnStartup);
        SubscribeLocalEvent<StellarEngineTunerComponent, StellarEngineRelayedEvent<StellarEngineShutdownEvent>>(OnShutdown);
        SubscribeLocalEvent<StellarEngineTunerComponent, StellarEngineRelayedEvent<StellarEngineChargedEvent>>(OnCharged);
        SubscribeLocalEvent<StellarEngineTunerComponent, StellarEngineRelayedEvent<StellarEngineUpdateEvent>>(OnUpdate);
        SubscribeLocalEvent<StellarEngineTunerComponent, StellarEngineRelayedEvent<StellarEngineGracePeriodEndEvent>>(OnGracePeriodEnd);
    }

    private void OnEndCollide(Entity<StellarEngineTunerComponent> ent, ref EndCollideEvent args)
    {
        if (!_doAfter.IsRunning(ent.Comp.ActiveDoAfterId) || ent.Comp.ActiveDoAfterId?.Uid != args.OtherEntity || ent.Comp.ContactPadFixture != args.OurFixtureId)
            return;

        _doAfter.Cancel(ent.Comp.ActiveDoAfterId);
        ent.Comp.ActiveDoAfterId = null;
        Dirty(ent);
    }

    private void OnStartCollide(Entity<StellarEngineTunerComponent> ent, ref StartCollideEvent args)
    {
        if (!_mobState.IsAlive(args.OtherEntity) || _doAfter.IsRunning(ent.Comp.ActiveDoAfterId) || !ent.Comp.Enabled || ent.Comp.TunerBroken || args.OurFixtureId != ent.Comp.ContactPadFixture)
            return;

        var doArgs = new DoAfterArgs(EntityManager, args.OtherEntity, TimeSpan.FromSeconds(ent.Comp.BaseTime * ent.Comp.DoAfterSpeedMult), new StellarEngineTunerDoAfter(), ent, args.OtherEntity)
        {
            NeedHand = true,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
            BreakOnWeightlessMove = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
            BreakOnDamage = true,
            RequireCanInteract = true,
            MovementThreshold = 0.35f,
        };
        _doAfter.TryStartDoAfter(doArgs, out var doAfterId);
        ent.Comp.ActiveDoAfterId = doAfterId;
        Dirty(ent);
    }

    private void OnGracePeriodEnd(Entity<StellarEngineTunerComponent> ent, ref StellarEngineRelayedEvent<StellarEngineGracePeriodEndEvent> args)
    {
        SetPadEnabled(ent, true);
        ent.Comp.TunerCurrentCalibration = ent.Comp.TunerCalibrationWindowGreen.Min; // As a freebie, all tuners start off auto-calibrated to green when everything resumes.
        ent.Comp.CalibrationWiggleCurrentBuildup = 0f; // Reset wiggle buildup to prevent insta-decalibration craziness.
        ent.Comp.TunerIncrementCalibrationValue = ent.Comp.TunerIncrementCalibrationValueMax;
        Dirty(ent);
        HandleTunerState(ent, null);
    }

    private void OnUpdate(Entity<StellarEngineTunerComponent> ent, ref StellarEngineRelayedEvent<StellarEngineUpdateEvent> args)
    {
        var decay = 0f;

        if (ent.Comp.TunerCurrentCalibration > 1f) // Don't build up Wiggle if the Tuner doesn't have any calibration.
        {
            var buildupSpeed = ent.Comp.State == TunerState.NotCalibrated ? ent.Comp.CalibrationWiggleBuildupSpeed * 0.5f : ent.Comp.CalibrationWiggleBuildupSpeed; // Only build up decay at half speed if we're in the red.
            ent.Comp.CalibrationWiggleCurrentBuildup += (_random.Prob(ent.Comp.CalibrationDecayBuildupChance)) ? buildupSpeed * args.Args.TimeDelta : 0;
        }

        if (ent.Comp.CalibrationWiggleCurrentBuildup >= ent.Comp.CalibrationWiggleBuildupNeeded)
        {
            ent.Comp.CalibrationWiggleCurrentBuildup = 0;
            decay = ent.Comp.CalibrationWiggleDecayStrength;
            _popup.PopupCoordinates(Loc.GetString("popup-stellarator-tuner-decayed"), Transform(ent).Coordinates, PopupType.LargeCaution);
        }
        var passiveDecay = Math.Clamp(ent.Comp.CalibrationPassiveDecay, 0, ent.Comp.TunerIncrementCalibrationValue);
        var calibrationSum = Math.Clamp(passiveDecay, 0, ent.Comp.TunerIncrementCalibrationValue);
        ent.Comp.TunerCurrentCalibration += calibrationSum * ent.Comp.CalibrationDecayMult * args.Args.TimeDelta;
        ent.Comp.TunerCurrentCalibration += decay * ent.Comp.CalibrationDecayMult; // Doesn't use timedelta.
        ent.Comp.TunerCurrentCalibration = Math.Clamp(ent.Comp.TunerCurrentCalibration, ent.Comp.TunerMinValue, ent.Comp.TunerMaxValue);
        ent.Comp.TunerIncrementCalibrationValue = MathHelper.Lerp(ent.Comp.TunerIncrementCalibrationValueMin, ent.Comp.TunerIncrementCalibrationValueMax, args.Args.LerpCalibration);
        Dirty(ent);

        switch (HandleTunerState(ent))
        {
            case TunerState.NearCalibrated:
                args.Args = args.Args with { NearCalibrated = args.Args.NearCalibrated + 1 };
                break;
            case TunerState.Calibrated:
                args.Args = args.Args with { Calibrated = args.Args.Calibrated + 1 };
                break;
            default:
                args.Args = args.Args with { NotCalibrated = args.Args.NotCalibrated + 1 };
                break;
        }
    }

    private void OnCharged(Entity<StellarEngineTunerComponent> ent, ref StellarEngineRelayedEvent<StellarEngineChargedEvent> args)
    {
        SetPadEnabled(ent, false);
        _light.SetColor(ent, Color.Black);
        _appearance.SetData(ent, TunerVisuals.Tuner, TunerState.Off);
        _appearance.SetData(ent, TunerVisuals.Venting, TunerVentState.Inert);
        ent.Comp.TunerCurrentCalibration = 50f; // This is off
        Dirty(ent);
    }

    private void OnStartup(Entity<StellarEngineTunerComponent> ent, ref StellarEngineRelayedEvent<StellarEngineStartupEvent> args)
    {
        SetPadEnabled(ent, true);
        ent.Comp.CalibrationPassiveDecay = args.Args.FuelRod.Comp.CalibrationPassiveDecay;
        ent.Comp.CalibrationWiggleDecayStrength = args.Args.FuelRod.Comp.CalibrationWiggleDecayStrength;
        ent.Comp.CalibrationDecayBuildupChance = args.Args.FuelRod.Comp.CalibrationDecayBuildupChance;
        ent.Comp.CalibrationWiggleBuildupSpeed = args.Args.FuelRod.Comp.CalibrationWiggleBuildupSpeed;
        ent.Comp.CalibrationWiggleBuildupNeeded = args.Args.FuelRod.Comp.CalibrationWiggleBuildupNeeded;
        ent.Comp.TunerIncrementCalibrationValue = ent.Comp.TunerIncrementCalibrationValueMax;
        HandleTunerState(ent);
    }

    private void OnShutdown(Entity<StellarEngineTunerComponent> ent, ref StellarEngineRelayedEvent<StellarEngineShutdownEvent> args)
    {
        SetPadEnabled(ent, false);
        _light.SetColor(ent, Color.Black);
        _appearance.SetData(ent, TunerVisuals.Tuner, TunerState.Off);
        ent.Comp.TunerCurrentCalibration = 0f;
        Dirty(ent);
    }

    private void BreakTuner(Entity<StellarEngineTunerComponent> ent)
    {
        EnsureComp<WeldableComponent>(ent, out var weldable);
        ent.Comp.TunerBroken = true;
        weldable.IsWelded = false;
        _sparks.DoSparks(ent, amount: 7, randomize: true);
        _appearance.SetData(ent, TunerVisuals.Broken, TunerBaseState.Broken);
        //TODO: Audio for tuner breaking

        if (_doAfter.IsRunning(ent.Comp.ActiveDoAfterId))
        {
            _doAfter.Cancel(ent.Comp.ActiveDoAfterId);
        }
    }

    private void OnChaosEvent(Entity<StellarEngineTunerComponent> ent, ref StellarEngineRelayedEvent<StellarEngineChaosEvent> args)
    {
        if (ent.Comp.TunerBroken || !_random.Prob(args.Args.Intensity * ent.Comp.ChaosEventProbability))
            return;

        BreakTuner(ent);
    }

    private void OnTunerAttemptWeld(Entity<StellarEngineTunerComponent> ent, ref WeldableAttemptEvent args)
    {
        if (!ent.Comp.TunerBroken)
            args.Cancel();
    }

    private void OnTunerWelded(Entity<StellarEngineTunerComponent> ent, ref WeldableChangedEvent args)
    {
        if (!args.IsWelded)
            return; // What?

        ent.Comp.TunerBroken = false;
        _appearance.SetData(ent, TunerVisuals.Broken, TunerBaseState.Intact);
        HandleTunerState(ent);
    }

    /// <summary>
    /// I hate this, it's gross.
    /// </summary>
    private TunerState HandleTunerState(Entity<StellarEngineTunerComponent> ent, bool? off = false)
    {
        var newState = TunerState.Off;
        var lightColor = Color.FromHex("#000000");
        if (ent.Comp.TunerCurrentCalibration > ent.Comp.TunerCalibrationWindowGreen.Min && ent.Comp.TunerCurrentCalibration < ent.Comp.TunerCalibrationWindowGreen.Max)
        {
            lightColor = Color.FromHex("#64D89C");
            newState = TunerState.Calibrated;
        }
        else if (ent.Comp.TunerCurrentCalibration > ent.Comp.TunerCalibrationWindowYellow.Min && ent.Comp.TunerCurrentCalibration < ent.Comp.TunerCalibrationWindowYellow.Max)
        {
            lightColor = Color.FromHex("#D4AA4B");
            newState = TunerState.NearCalibrated;
        }
        else if (off == false)
        {
            lightColor = Color.FromHex("#D60E4A");
            newState = TunerState.NotCalibrated;
        }

        if (newState == ent.Comp.State) // If the state is the same as before, bail to avoid running unneccesary code.
            return ent.Comp.State;
        if (ent.Comp.TunerCurrentCalibration > ent.Comp.TunerCalibrationWindowGreen.Max)
            _appearance.SetData(ent, TunerVisuals.Venting, TunerVentState.Venting);
        else
            _appearance.SetData(ent, TunerVisuals.Venting, TunerVentState.Inert);

        _light.SetColor(ent, lightColor);

        _appearance.SetData(ent, TunerVisuals.Tuner, newState);
        ent.Comp.State = newState;
        return newState;
    }

    private void OnTuningDoAfter(Entity<StellarEngineTunerComponent> ent, ref StellarEngineTunerDoAfter args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.TunerBroken)
            return;

        var sfxParams = ent.Comp.CalibrateSfx.Params;
        var pitch = (int)Math.Round(ent.Comp.TunerCurrentCalibration / ent.Comp.TunerMaxValue * 24 - 12); // Make the 0-100 a -12 to +12 range for the semitone scale.

        args.Repeat = true;
        args.Handled = true;
        sfxParams = AudioHelpers.ShiftSemitone(sfxParams, pitch).AddVolume(-10f);
        sfxParams.MaxDistance = 5.5f;
        _audio.PlayPredicted(ent.Comp.CalibrateSfx, ent, args.User, sfxParams);

        var oldValue = ent.Comp.TunerCurrentCalibration;
        var newValue = ent.Comp.TunerCurrentCalibration += ent.Comp.TunerIncrementCalibrationValue;

        if (oldValue < ent.Comp.TunerCalibrationWindowGreen.Max && newValue > ent.Comp.TunerCalibrationWindowGreen.Max)
            _appearance.SetData(ent, TunerVisuals.Venting, TunerVentState.Venting);

        ent.Comp.TunerCurrentCalibration = Math.Clamp(newValue, ent.Comp.TunerMinValue, ent.Comp.TunerMaxValue);
        Dirty(ent);
    }

    private void SetPadEnabled(Entity<StellarEngineTunerComponent> ent, bool enabled)
    {
        if (_doAfter.IsRunning(ent.Comp.ActiveDoAfterId))
            _doAfter.Cancel(ent.Comp.ActiveDoAfterId);
        ent.Comp.Enabled = enabled;
        Dirty(ent);
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarEngineTunerDoAfter : SimpleDoAfterEvent
{
    public override DoAfterEvent Clone() => this;
}
