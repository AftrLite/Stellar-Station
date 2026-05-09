// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Popups;
using Content.Stellar.Shared.Science;

namespace Content.Stellar.Server.Science;

public sealed class StellarScienceAnalystSystem : SharedStellarScienceAnalystSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSensorTowerComponent, StellarSensorTowerDoAfter>(OnTowerDoAfter);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var terminalQuery = EntityQueryEnumerator<StellarSensorTerminalComponent>();
        while (terminalQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.State != StellarSensorTerminalState.Idle && comp.TimeOutTimer <= Timing.CurTime && comp.ConnectedTower != null)
            {
                ResetConnection(comp.ConnectedTower.Value, (uid, comp));
                Popup.PopupPredicted("Synchronization timeout.", uid, uid, PopupType.MediumCaution); // TODO: Localization
                Popup.PopupPredicted("Synchronization timeout.", comp.ConnectedTower.Value, comp.ConnectedTower.Value, PopupType.LargeCaution); // TODO: Localization
            }
        }
    }


    private void OnTowerDoAfter(Entity<StellarSensorTowerComponent> ent, ref StellarSensorTowerDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Repeat = true;
    }
}
