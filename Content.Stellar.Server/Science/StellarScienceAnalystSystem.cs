// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Stellar.Shared.Science;
using Robust.Shared.Random;

namespace Content.Stellar.Server.Science;

public sealed class StellarScienceAnalystSystem : SharedStellarScienceAnalystSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

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
                Popup.PopupPredicted("Synchronization timeout.", uid, uid, PopupType.MediumCaution); // TODO: Localization
                Popup.PopupPredicted("Synchronization timeout.", comp.SyncingTower.Value, comp.SyncingTower.Value, PopupType.LargeCaution); // TODO: Localization
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
                    // TODO: Localized popup that makes it obvious the damage caused a desync.
                    ent.Comp.State = StellarSensorTowerState.Idle;
                    break;
                }
                ent.Comp.Damaged = true; // TODO: Visuals & such.
                break;
            case StellarSensorTowerEffect.AttractMobs:
            {
                // TODO: Spawn a buncha nasties. Might need new logic to spawn hazard-sector-specific mobs.
            }
                break;
            case StellarSensorTowerEffect.Desync:
                repeat = false;
                ent.Comp.State = StellarSensorTowerState.Idle;
                break;
        }

        if (ent.Comp.Progress >= 100f)
        {
            repeat = false;
            ent.Comp.Progress = 0f;
            ent.Comp.State = StellarSensorTowerState.FullDrive;
            // TODO: Desync the tower, set up tower for its drive to be ejected. THAT'S RIGHT, IT'S CONTAINER TIME.
        }

        Dirty(ent);
        args.Repeat = repeat;
    }

    private void OnTowerInit(Entity<StellarSensorTowerComponent> ent, ref MapInitEvent args)
    {
        var mapId  = Transform(ent).MapID;
        var gridUid = Transform(ent).GridUid;

        _terminal.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, 500, _terminal);
        foreach (var core in  _terminal)
        {
            var candidateMapId = Transform(core).MapID;
            var candidateGridUid = Transform(core).GridUid;
            if (candidateMapId != mapId || candidateGridUid != gridUid)
                continue;

            ent.Comp.LinkedTerminal = core;
            core.Comp.LinkedTowers.Add(ent);
            Dirty(core);
            Dirty(ent);
            break;
        }
    }
}
