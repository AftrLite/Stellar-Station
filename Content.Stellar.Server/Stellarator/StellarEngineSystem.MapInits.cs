// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.Stellarator;

namespace Content.Stellar.Server.Stellarator;

public sealed partial class StellarEngineSystem
{
    private readonly HashSet<Entity<StellarEngineCoreComponent>> _core = [];

    private void OnPartInit(Entity<StellarEnginePartComponent> ent, ref MapInitEvent args)
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
