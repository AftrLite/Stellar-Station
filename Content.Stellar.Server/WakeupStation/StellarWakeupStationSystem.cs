// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Events;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.EntitySystems;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared._ES.Camera;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.StatusEffectNew;
using Content.Stellar.Shared._ES.Core.Timer;
using Content.Stellar.Shared.CCVars;
using Robust.Server.Containers;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Stellar.Server.WakeupStation;

public sealed class StellarWakeupStationSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly DockingSystem _dock = default!;
    [Dependency] private readonly ESEntityTimerSystem _esTimer = default!;
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly SharedPoweredLightSystem _lights = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly ThirstSystem _thirst = default!;

    private bool _stationWakeupEnabled;
    private float _stationWakeupTime;
    private float _stationSleepTime;

    private readonly HashSet<Entity<PoweredLightComponent, TransformComponent>> _lightSet = new();
    private static readonly EntProtoId SleepStatusEffect = "StatusEffectForcedSleeping";

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialized);
        SubscribeLocalEvent<StellarWakeupStationComponent, StationPostInitEvent>(OnStationPostInit);
        SubscribeLocalEvent<StellarWakeupStationComponent, FTLCompletedEvent>(OnFTLCompleted);

        SubscribeLocalEvent<PlayerSpawningEvent>(HandlePlayerSpawning, before: [typeof(SpawnPointSystem)]);

        _config.OnValueChanged(STCCVars.StationWakeupEnabled, OnWakeupConfigChanged, true);
        _config.OnValueChanged(STCCVars.StationWakeupTime, (f => _stationWakeupTime = f), true);
        _config.OnValueChanged(STCCVars.StationSleepTime, (f => _stationSleepTime = f), true);
    }

    private void OnWakeupConfigChanged(bool val)
    {
        if (_stationWakeupEnabled && !val && _ticker.RunLevel != GameRunLevel.PreRoundLobby)
        {
            Log.Error("Kazne didn't bother implementing disabling station wakeup mid-round. Lol, lmao even.");
            return;
        }

        _stationWakeupEnabled = val;
    }

    private void OnStationInitialized(StationInitializedEvent msg)
    {
        if (!_stationWakeupEnabled)
            return;

        EnsureComp<StellarWakeupStationComponent>(msg.Station);
    }

    private void OnStationPostInit(Entity<StellarWakeupStationComponent> ent, ref StationPostInitEvent args)
    {
        var stationGrid = _station.GetLargestGrid(ent.Owner);
        if (stationGrid == null)
            return;

        ent.Comp.GridUid = stationGrid.Value;

        _lightSet.Clear();
        _dock.SetDockBolts(ent.Comp.GridUid.Value, true);
        _lookup.GetChildEntities(ent.Comp.GridUid.Value, _lightSet);
        foreach (var light in _lightSet)
        {
            _lights.SetState(light, false); // Turn all the lights off
            _esTimer.SpawnMethodTimer(TimeSpan.FromSeconds(_random.NextFloat(_stationWakeupTime / 10, _stationWakeupTime)), () => { _lights.SetState(light, true); }); //TODO: Reflect hazard sector travel time for lights turning on
        }
    }

    private void HandlePlayerSpawning(PlayerSpawningEvent ev)
    {
        if (ev.SpawnResult != null)
            return;

        if (!_stationWakeupEnabled)
            return;

        if (!TryComp<StellarWakeupStationComponent>(ev.Station, out var station) || station.GridUid is not { } grid)
            return;

        var points = EntityQueryEnumerator<CryostorageComponent, TransformComponent>();
        var possiblePositions = new List<(EntityUid, BaseContainer)>();
        while (points.MoveNext(out var uid, out var cryo, out var xform))
        {
            if (xform.GridUid != grid)
                continue;

            if (!_container.TryGetContainer(uid, cryo.ContainerId, out var container) || container.ContainedEntities.Any())
                continue;

            possiblePositions.Add((uid, container));
        }

        var (cryostorage, cryoContainer) = possiblePositions.Count > 0 ? _random.Pick(possiblePositions) : (EntityUid.Invalid, default);
        var spawnLoc = cryostorage.Valid ? Transform(cryostorage).Coordinates : new EntityCoordinates(grid, 0, 0);

        ev.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            ev.Job,
            ev.HumanoidCharacterProfile,
            ev.Station);

        if (cryostorage.Valid)
            _container.Insert(ev.SpawnResult.Value, cryoContainer!);

        if (TryComp<HungerComponent>(ev.SpawnResult, out var hunger) && hunger.Thresholds.TryGetValue(HungerThreshold.Starving, out var starving))
            _hunger.SetHunger(ev.SpawnResult.Value, starving + _random.NextFloat(-20, 0), hunger);

        if (TryComp<ThirstComponent>(ev.SpawnResult, out var thirst) && thirst.ThirstThresholds.TryGetValue(ThirstThreshold.Parched, out var parched))
            _thirst.SetThirst(ev.SpawnResult.Value, thirst, parched + _random.NextFloat(-50, 0));

        if (_timing.CurTime - _ticker.RoundStartTimeSpan < TimeSpan.FromSeconds(90)) // The station's arrived, so people can wake up quick.
            _statusEffects.TryAddStatusEffectDuration(ev.SpawnResult.Value, SleepStatusEffect, TimeSpan.FromSeconds(1));
        else
            _statusEffects.TryAddStatusEffectDuration(ev.SpawnResult.Value, SleepStatusEffect, TimeSpan.FromSeconds(_random.NextFloat(_stationSleepTime / 10f, _stationSleepTime)));
    }

    private void OnFTLCompleted(Entity<StellarWakeupStationComponent> ent, ref FTLCompletedEvent args)
    {
        if (ent.Comp.GridUid is not { } grid)
            return; // bruh

        var translation = new ESScreenshakeParameters() { Trauma = 2.8f, DecayRate = 0.04f, Frequency = 0.015f };
        var filter = Filter.BroadcastGrid(ent.Comp.GridUid.Value);
        _shake.Screenshake(filter, translation, null);
        _shuttle.Disable(ent); // Stations don't need to move, dummy. This permanently anchors it and eliminates the need for Station Anchors.
    }
}
