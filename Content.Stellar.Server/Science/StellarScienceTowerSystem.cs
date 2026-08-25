// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared._ES.Sparks;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Tools.Components;
using Content.Stellar.Shared.HazardSectors;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Server.Science;

public sealed class StellarScienceTowerSystem : SharedStellarScienceTowerSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ESSparksSystem _sparks = default!;

    private readonly HashSet<Entity<StellarSensorTerminalComponent>> _terminal = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSensorTowerComponent, StellarSensorTowerDoAfter>(OnTowerDoAfter);
        SubscribeLocalEvent<StellarSensorTowerComponent, MapInitEvent>(OnTowerInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var terminalQuery = EntityQueryEnumerator<StellarSensorTerminalComponent>();
        while (terminalQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.State != StellarSensorTerminalState.Idle && comp.TimeOutTimer <= Timing.CurTime && comp.SyncingTower != null)
            {
                Popup.PopupPredicted("sensortower-popup-timeout", uid, uid, PopupType.MediumCaution);
                Popup.PopupPredicted("sensortower-popup-timeout", comp.SyncingTower.Value, comp.SyncingTower.Value, PopupType.LargeCaution);
                ResetConnection(comp.SyncingTower.Value, (uid, comp));
            }
        }
    }

    private void OnTowerDoAfter(Entity<StellarSensorTowerComponent> ent, ref StellarSensorTowerDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        ent.Comp.Progress += Random.NextFloat(ent.Comp.ProgressGainMin, ent.Comp.ProgressGainMax);
        Math.Clamp(ent.Comp.Progress, 0f, 100f);

        var repeat = true;
        var effect = Random.Pick(ent.Comp.EffectProbabilities);
        Log.Info($"{effect}");
        switch (effect)
        {
            case StellarSensorTowerEffect.Damage:
                if (ent.Comp.Damaged)
                {
                    repeat = false;
                    Ambient.SetAmbience(ent, false);
                    Popup.PopupPredicted("sensortower-popup-desync-damage", ent, ent, PopupType.LargeCaution);
                    ent.Comp.State = StellarSensorTowerState.Idle;
                    break;
                }
                EnsureComp<WeldableComponent>(ent, out var weldable);
                weldable.IsWelded = false;
                ent.Comp.Damaged = true;
                Appearance.SetData(ent, StellarSensorTowerVisuals.Visuals, 2);
                _sparks.DoSparks(ent, amount: 5, randomize: true);
                break;
            case StellarSensorTowerEffect.AttractMobs:
            {
                for (var i = 0; i < 6;)
                {
                    if (TrySpawnAtBeacon(ent))
                        i++;
                }
            }
                break;
            case StellarSensorTowerEffect.Desync:
                repeat = false;
                Ambient.SetAmbience(ent, false);
                Popup.PopupPredicted("sensortower-popup-desync", ent, ent, PopupType.LargeCaution);
                ent.Comp.State = StellarSensorTowerState.Idle;
                break;
        }

        if (ent.Comp.Progress >= 100f)
        {
            repeat = false;
            ent.Comp.Progress = 0f;
            ent.Comp.State = StellarSensorTowerState.FullDrive;
            RaiseNetworkEvent(new StellarTowerCompleteVisualsEvent(GetNetEntity(ent)));
            Ambient.SetAmbience(ent, false);
            Audio.PlayPredicted(ent.Comp.FinishSound, ent, args.User);
        }

        Dirty(ent);
        args.Repeat = repeat;
    }

    private bool TrySpawnAtBeacon(EntityUid uid)
    {
        var xform = Transform(uid);
        var offsetX = Random.NextFloat(-7f, 7f);
        var offsetY = Random.NextFloat(-7f, 7f);
        var anchorGrid = xform.GridUid ?? xform.MapUid;

        if (!TryComp<StellarHazardSectorStationComponent>(anchorGrid, out var sector) || !anchorGrid.Value.IsValid())
            return false;

        var targetCoords = new EntityCoordinates(anchorGrid.Value, xform.Coordinates.Position + new Vector2(offsetX, offsetY));
        if (targetCoords.EntityId == EntityUid.Invalid)
            return false;

        if (sector.SectorMobs.Capacity > 0)
        {
            var toSpawn = Random.Pick(sector.SectorMobs).Id;
            Spawn(toSpawn, targetCoords);
            return true;
        }
        return false;
    }

    private void OnTowerInit(Entity<StellarSensorTowerComponent> ent, ref MapInitEvent args)
    {
        var mapId  = Transform(ent).MapID;
        var gridUid = Transform(ent).GridUid;

        _terminal.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, 500, _terminal);
        foreach (var terminal in  _terminal)
        {
            var candidateMapId = Transform(terminal).MapID;
            var candidateGridUid = Transform(terminal).GridUid;
            if (candidateMapId != mapId || candidateGridUid != gridUid)
                continue;

            ent.Comp.LinkedTerminal = terminal;
            terminal.Comp.LinkedTowers.Add(ent);
            Dirty(terminal);
            Dirty(ent);
            break;
        }
    }
}
