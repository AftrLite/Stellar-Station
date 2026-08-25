// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Linq;
using Content.Shared.Humanoid;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Enums;

namespace Content.Stellar.Server.Science;

public sealed partial class StellarScienceBluespaceDriveSystem
{
    private readonly HashSet<Entity<StellarBluespaceDriveCoreComponent>> _core = [];

    private void OnCoreInit(Entity<StellarBluespaceDriveCoreComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.TotalCrew = _playerMan.Sessions.Count(session => session.Status == SessionStatus.InGame && HasComp<HumanoidProfileComponent>(session.AttachedEntity));
        ent.Comp.AutoLeaveTimer = _leaveTime + _timing.CurTime;
        ent.Comp.StartupTimer = _startupTime + _timing.CurTime;
        Dirty(ent);
    }

    private void OnPartInit(Entity<StellarBluespaceDrivePartComponent> ent, ref MapInitEvent args)
    {
        var mapId  = Transform(ent).MapID;
        var gridUid = Transform(ent).GridUid;

        _core.Clear();
        _lookup.GetEntitiesInRange(Transform(ent).Coordinates, 50, _core);
        foreach (var core in  _core)
        {
            var candidateMapId = Transform(core).MapID;
            var candidateGridUid = Transform(core).GridUid;
            if (candidateMapId != mapId || candidateGridUid != gridUid)
                continue;

            ent.Comp.LinkedCore = core;
            core.Comp.LinkedParts.Add(ent);
            Dirty(core);
            Dirty(ent);
            break;
        }
    }
}
