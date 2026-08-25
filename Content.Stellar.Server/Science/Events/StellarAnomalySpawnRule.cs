// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Server.Atmos.EntitySystems;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.CCVar;
using Content.Shared.GameTicking.Components;
using Content.Shared.Physics;
using Content.Stellar.Server.HazardSectors;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Stellar.Server.Science.Events;

public sealed class StellarAnomalySpawnRule : StellarGameRuleSystem<StellarAnomalySpawnRuleComponent>
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;

    protected override void Started(EntityUid uid, StellarAnomalySpawnRuleComponent comp, GameRuleComponent rule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, rule, args);

        if (!TryGetRandomStation(out var station))
            return;

        var grid = _station.GetLargestGrid(station.Value);
        if (grid == null)
            return;

        var toSpawn = 1;
        var intensity = GetShiftIntensity();
        var playersInGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
        var announcement = Loc.GetString("stellar-anomaly-spawn-event", ("sighting", Loc.GetString($"anomaly-sighting-{_random.Next(1, 11)}")));
        _chatSystem.DispatchFilteredAnnouncement(playersInGame, announcement, sender: Loc.GetString("stellar-event-sender"), colorOverride: Color.FromHex("#cae8e8"));

        switch (intensity.ID)
        {
            case "StellarIntensityGreen":
                toSpawn = _random.Next(1, 3);
                break;
            case "StellarIntensityYellow":
                toSpawn = _random.Next(1, 4);
                break;
            case "StellarIntensityOrange":
                toSpawn = _random.Next(2, 4);
                break;
            case "StellarIntensityRed":
                toSpawn = _random.Next(2, 5);
                break;
            case "StellarIntensityBlack":
                toSpawn = _random.Next(3, 5);
                break;
            default:
                toSpawn = 1;
                break;
        }

        for (var i = 0; i < toSpawn; i++)
        {
            var anom = _random.Pick(GetSectorAnomalies()).Key;
            SpawnOnRandomGridLocation(grid.Value, anom);
        }
    }

    private void SpawnOnRandomGridLocation(EntityUid grid, string toSpawn)
    {
        if (!TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var xform = Transform(grid);
        var position = xform.Coordinates;
        var bounds = gridComp.LocalAABB.Scale(_config.GetCVar(CCVars.AnomalyGenerationGridBoundsScale));

        for (var i = 0; i < 25; i++)
        {
            var randomX = Random.Next((int) bounds.Left, (int) bounds.Right);
            var randomY = Random.Next((int) bounds.Bottom, (int)bounds.Top);
            var tile = new Vector2i(randomX, randomY);
            var valid = true;

            if (_atmos.IsTileSpace(grid, xform.MapUid, tile) || _atmos.IsTileAirBlockedCached(grid, tile))
                continue;

            var physQuery = GetEntityQuery<PhysicsComponent>();

            foreach (var ent in _mapSystem.GetAnchoredEntities(grid, gridComp, tile)) // This should be using static lookup, honestly.
            {
                if (!physQuery.TryGetComponent(ent, out var body))
                    continue;
                if (body.BodyType != BodyType.Static || !body.Hard || (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }
            if (!valid)
                continue;

            position = _mapSystem.GridTileToLocal(grid, gridComp, tile);
            break;
        }

        Spawn(toSpawn, position);
    }
}
