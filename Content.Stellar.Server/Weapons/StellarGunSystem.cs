// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.Projectiles;
using Content.Stellar.Shared.Weapons;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Server.Weapons;

public sealed partial class StellarGunSystem : SharedStellarGunSystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var regenQuery = EntityQueryEnumerator<StellarAmmoRegenComponent>();
        while (regenQuery.MoveNext(out var uid, out var comp))
        {
            if (Timing.CurTime >= comp.RegenTime)
            {
                var done = false;
                if (TryComp<StellarGunReloadableComponent>(uid, out var gunComp) && gunComp.AmmoReserves < gunComp.AmmoMaxReserves)
                {
                    done = true;
                    gunComp.AmmoReserves = Math.Clamp(gunComp.AmmoReserves.Value + comp.AmmoRegenerated, 0, gunComp.AmmoMaxReserves.Value);
                    Dirty(uid, gunComp);
                }

                if (TryComp<StellarAmmoComponent>(uid, out var entComp) && entComp.CurrentAmmo < entComp.MaxAmmo)
                {
                    done = true;
                    entComp.CurrentAmmo = Math.Clamp(entComp.CurrentAmmo.Value + comp.AmmoRegenerated, 0, entComp.MaxAmmo.Value);
                    Dirty(uid, entComp);
                }

                if (done)
                {
                    PopUp.PopupEntity(Loc.GetString("stellar-ammo-regen", ("count", comp.AmmoRegenerated)), uid);
                    Audio.PlayPredicted(comp.SoundOnRegen, uid, uid);
                }

                comp.RegenTime = Timing.CurTime + comp.RegenInterval;
                Dirty(uid, comp);
            }
        }
    }

    protected override void StellarHitscan(EntityUid gunUid, StellarHitscanEvent message, EntityUid? user = null)
    {
        var filter = Filter.Pvs(gunUid, entityManager: EntityManager);

        if (TryComp<ActorComponent>(user, out var actor))
            filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(message, filter);
    }

    protected override void StellarMuzzleFlash(EntityUid gunUid, StellarMuzzleFlashEvent message, EntityUid? user = null)
    {
        var filter = Filter.Pvs(gunUid, entityManager: EntityManager);

        if (TryComp<ActorComponent>(user, out var actor))
            filter.RemovePlayer(actor.PlayerSession);

        RaiseNetworkEvent(message, filter);
    }
}
