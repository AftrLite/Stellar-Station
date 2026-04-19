// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Weapons;

public abstract partial class SharedStellarGunSystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private void InitializeTypes()
    {
        SubscribeLocalEvent<StellarAmmoComponent, MapInitEvent>(OnAmmoInit);
        SubscribeLocalEvent<StellarAmmoRegenComponent, MapInitEvent>(OnRegenInit);
        SubscribeLocalEvent<StellarGunReloadableComponent, MapInitEvent>(OnReloadableInit);

        SubscribeLocalEvent<StellarAmmoComponent, InteractUsingEvent>(OnAmmoInteractUsing);
        SubscribeLocalEvent<StellarGunReloadableComponent, InteractUsingEvent>(OnReloadableInteractUsing);
        SubscribeLocalEvent<StellarGunReloadableComponent, StellarAmmoSourceDoAfter>(OnAmmoSourceDoAfter);
        SubscribeLocalEvent<StellarGunReloadableComponent, GetAmmoCountEvent>(OnReloadableAmmoCount);
        SubscribeLocalEvent<StellarGunReloadableComponent, TakeAmmoEvent>(OnAmmoUsed);

        SubscribeLocalEvent<StellarAmmoComponent, ExaminedEvent>(OnAmmoExamined);
        SubscribeLocalEvent<StellarAmmoRegenComponent, ExaminedEvent>(OnRegenExamined);
        SubscribeLocalEvent<StellarGunReloadableComponent, ExaminedEvent>(OnReloadableExamined);
    }

    # region MapInits
    private void OnRegenInit(Entity<StellarAmmoRegenComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.RegenTime = Timing.CurTime + ent.Comp.RegenInterval;
        Dirty(ent);
    }

    private void OnReloadableInit(Entity<StellarGunReloadableComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.AmmoName is null && _proto.TryIndex(ent.Comp.WeaponType, out var proto))
        {
            ent.Comp.AmmoName = proto.Ammo;
            ent.Comp.AmmoSuffix = proto.Suffix;
        }

        if (ent.Comp.AmmoCount is null)
            ent.Comp.AmmoCount = ent.Comp.AmmoMagCapacity;

        if (ent.Comp.AmmoReserves is null)
            ent.Comp.AmmoReserves = ent.Comp.AmmoMaxReserves;

        Dirty(ent);
        UpdateReloadableAppearance(ent);
    }

    private void OnAmmoInit(Entity<StellarAmmoComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.AmmoName is null && _proto.TryIndex(ent.Comp.WeaponType, out var proto))
        {
            ent.Comp.AmmoName = proto.Ammo;
            ent.Comp.AmmoSuffix = proto.Suffix;
        }

        if (ent.Comp.CurrentAmmo is null)
            ent.Comp.CurrentAmmo = ent.Comp.MaxAmmo;

        Dirty(ent);
    }
    #endregion

    private void OnAmmoSourceDoAfter(Entity<StellarGunReloadableComponent> ent, ref StellarAmmoSourceDoAfter args)
    {
        if (args.Handled || args.Cancelled || args.Used is null)
            return;

        if (!TryComp<StellarAmmoComponent>(args.Used, out var ammoSource)
            || ent.Comp.WeaponType != ammoSource.WeaponType)
            return;

        var handled = TryTransferAmmo(args.User, ammoSource.AmmoPerDoAfter, (args.Used.Value, ammoSource), ent);
        args.Handled = handled;
        if (ent.Comp.AmmoReserves != ent.Comp.AmmoMaxReserves && ammoSource.CurrentAmmo > ammoSource.MinAmmo)
            args.Repeat = handled;
    }

    private void OnReloadableInteractUsing(Entity<StellarGunReloadableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(ent) || TerminatingOrDeleted(args.Used))
            return;

        if (!TryComp<StellarAmmoComponent>(args.Used, out var ammoSource)
            || ent.Comp.WeaponType != ammoSource.WeaponType
            || ent.Comp.AmmoMaxReserves == ent.Comp.AmmoReserves)
            return;

        if (ammoSource.UsesDoAfter)
        {
            args.Handled = TryAmmoDoAfter(args.User, (args.Used, ammoSource), ent);
            return;
        }

        args.Handled = TryTransferAmmo(args.User, (args.Used, ammoSource), ent);
    }

    private void OnAmmoInteractUsing(Entity<StellarAmmoComponent> ammoTarget, ref InteractUsingEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(ammoTarget) || TerminatingOrDeleted(args.Used))
            return;

        if (ammoTarget.Comp.Behaviour == StellarAmmoBehaviour.Reloader
            && TryComp<StellarGunReloadableComponent>(args.Used, out var weaponSource)
            && ammoTarget.Comp.WeaponType == weaponSource.WeaponType
            && weaponSource.AmmoMaxReserves != weaponSource.AmmoReserves)
        {
            if (ammoTarget.Comp.UsesDoAfter)
            {
                args.Handled = TryAmmoDoAfter(args.User, args.Used, ammoTarget);
                return;
            }
            args.Handled = TryTransferAmmo(args.User, ammoTarget, (args.Used, weaponSource));
            return;
        }

        if (TryComp<StellarAmmoComponent>(args.Used, out var ammoSource) && ammoTarget.Comp.WeaponType == ammoSource.WeaponType)
            args.Handled = TryTransferAmmo(args.User, (args.Used, ammoSource), ammoTarget);
    }

    private bool TryAmmoDoAfter(EntityUid user, Entity<StellarAmmoComponent> ammoSource, Entity<StellarGunReloadableComponent> ammoTarget)
    {
        if (DoAfter.IsRunning(ammoSource.Comp.DoAfterId) && !_netManager.IsClient)
        {
            PopUp.PopupEntity(Loc.GetString("stellar-ammo-reloader-occupied"), user, user, PopupType.SmallCaution);
            return false;
        }
        if (!DoAfter.IsRunning(ammoSource.Comp.DoAfterId))
        {
            var doArgs = new DoAfterArgs(EntityManager, user, ammoSource.Comp.DoAfterTime, new StellarAmmoSourceDoAfter(), ammoTarget, user, ammoSource)
            {
                MovementThreshold = 0.15f,
                DistanceThreshold = 0.25f,
                BreakOnHandChange = true,
                BreakOnDropItem = true,
            };
            var handled = DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
            ammoSource.Comp.DoAfterId = doAfterId;
            return handled;
        }
        return false;
    }

    private bool TryAmmoDoAfter(EntityUid user, EntityUid used, Entity<StellarAmmoComponent> ammoSource)
    {
        if (DoAfter.IsRunning(ammoSource.Comp.DoAfterId) && !_netManager.IsClient)
        {
            PopUp.PopupEntity(Loc.GetString("stellar-ammo-reloader-occupied"), user, user, PopupType.SmallCaution);
            return false;
        }
        if (!DoAfter.IsRunning(ammoSource.Comp.DoAfterId))
        {
            var doArgs = new DoAfterArgs(EntityManager, user, ammoSource.Comp.DoAfterTime, new StellarAmmoSourceDoAfter(), used, user, ammoSource)
            {
                MovementThreshold = 0.15f,
                DistanceThreshold = 0.25f,
                BreakOnHandChange = true,
                BreakOnDropItem = true,
            };
            var handled = DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
            ammoSource.Comp.DoAfterId = doAfterId;
            return handled;
        }
        return false;
    }

    #region Ammo Handling Methods
    private bool TryTransferAmmo(EntityUid user, int amount, Entity<StellarAmmoComponent> ammoSource, Entity<StellarGunReloadableComponent> ammoTarget)
    {
        if (ammoTarget.Comp.AmmoMaxReserves is null
            || ammoTarget.Comp.AmmoReserves is null
            || ammoTarget.Comp.AmmoReserves == ammoTarget.Comp.AmmoMaxReserves
            || ammoSource.Comp.CurrentAmmo is null)
            return false;

        var weaponAmmoToTransfer = Math.Clamp(ammoTarget.Comp.AmmoMaxReserves.Value - ammoTarget.Comp.AmmoReserves.Value, 0, amount);

        if (!ModifyAmmoCount(ammoTarget.Owner, weaponAmmoToTransfer))
            return false;

        if (!ammoSource.Comp.InfiniteAmmo)
            ammoSource.Comp.CurrentAmmo -= weaponAmmoToTransfer;

        Dirty(ammoSource);

        if (ammoSource.Comp.CurrentAmmo <= 0)
            PredictedQueueDel(ammoSource);
        return true;
    }

    private bool TryTransferAmmo(EntityUid user, Entity<StellarAmmoComponent> ammoSource, Entity<StellarGunReloadableComponent> ammoTarget)
    {
        if (ammoTarget.Comp.AmmoMaxReserves is null
            || ammoTarget.Comp.AmmoReserves is null
            || ammoTarget.Comp.AmmoReserves == ammoTarget.Comp.AmmoMaxReserves
            || ammoSource.Comp.CurrentAmmo is null)
            return false;

        var weaponAmmoToTransfer = Math.Clamp(ammoTarget.Comp.AmmoMaxReserves.Value - ammoTarget.Comp.AmmoReserves.Value, 0, ammoSource.Comp.CurrentAmmo.Value);

        if (!ModifyAmmoCount(ammoTarget.Owner, weaponAmmoToTransfer))
            return false;

        if (!ammoSource.Comp.InfiniteAmmo)
            ammoSource.Comp.CurrentAmmo -= weaponAmmoToTransfer;

        Dirty(ammoSource);
        Audio.PlayPredicted(ammoSource.Comp.AmmoSound, user, user);
        if (ammoSource.Comp.CurrentAmmo <= 0)
            PredictedQueueDel(ammoSource);

        return true;
    }

    private bool TryTransferAmmo(EntityUid user, Entity<StellarAmmoComponent> ammoSource, Entity<StellarAmmoComponent> ammoTarget)
    {
        if (ammoTarget.Comp.CurrentAmmo is null || ammoTarget.Comp.MaxAmmo is null || ammoSource.Comp.CurrentAmmo is null || ammoSource.Comp.MaxAmmo is null)
            return false;

        switch (ammoTarget.Comp.Behaviour) // You'd think there'd be a smarter way of handling this, right? But i can't think of one!
        {
            case StellarAmmoBehaviour.Ammo:
                var ammoToTransfer = Math.Clamp(ammoTarget.Comp.MaxAmmo.Value - ammoTarget.Comp.CurrentAmmo.Value, 0, ammoSource.Comp.CurrentAmmo.Value);

                if (ammoTarget.Comp.CurrentAmmo == ammoTarget.Comp.MaxAmmo)
                    return false;

                if (!ammoSource.Comp.InfiniteAmmo)
                    ammoSource.Comp.CurrentAmmo -= ammoToTransfer;
                ammoTarget.Comp.CurrentAmmo += ammoToTransfer;

                Dirty(ammoTarget);
                Dirty(ammoSource);
                Audio.PlayPredicted(ammoSource.Comp.AmmoSound, user, user);
                if (ammoSource.Comp.CurrentAmmo <= 0)
                    PredictedQueueDel(ammoSource);
                return true;

            case StellarAmmoBehaviour.Reloader:
                var reloaderAmmoToTransfer = Math.Clamp(ammoSource.Comp.MaxAmmo.Value - ammoSource.Comp.CurrentAmmo.Value, 0, ammoTarget.Comp.CurrentAmmo.Value);

                if (ammoSource.Comp.CurrentAmmo == ammoSource.Comp.MaxAmmo)
                    return false;

                if (!ammoTarget.Comp.InfiniteAmmo)
                    ammoTarget.Comp.CurrentAmmo -= reloaderAmmoToTransfer;
                ammoSource.Comp.CurrentAmmo += reloaderAmmoToTransfer;

                Dirty(ammoTarget);
                Dirty(ammoSource);
                Audio.PlayPredicted(ammoTarget.Comp.AmmoSound, user, user); // Play the gun's reload sound rather than the Ammo's.
                return true; // don't QueueDel because reloaders shouldn't be destroyed by drawing ammo from them.
        }
        return false;
    }
    #endregion

    private void OnReloadableAmmoCount(Entity<StellarGunReloadableComponent> ent, ref GetAmmoCountEvent args)
    {
        args.Capacity = ent.Comp.AmmoMagCapacity ?? int.MaxValue;
        args.Count = ent.Comp.AmmoCount ?? int.MaxValue;
    }

    private void OnAmmoUsed(Entity<StellarGunReloadableComponent> ent, ref TakeAmmoEvent args)
    {
        if (ent.Comp.AmmoCount <= 0)
            return;

        if (ent.Comp.AmmoCount != null)
            ent.Comp.AmmoCount--;
        Dirty(ent);
        UpdateReloadableAppearance(ent);
    }

    private bool ModifyAmmoCount(Entity<StellarGunReloadableComponent?> ent, int delta)
    {
        if (!Resolve(ent, ref ent.Comp, false) || ent.Comp.AmmoReserves == null)
            return false;

        return UpdateReloadableCount((ent.Owner, ent.Comp), ent.Comp.AmmoReserves.Value + delta);
    }

    private bool UpdateReloadableCount(Entity<StellarGunReloadableComponent?> ent, int count)
    {
        if (!Resolve(ent, ref ent.Comp, false) || count > ent.Comp.AmmoMaxReserves)
            return false;

        ent.Comp.AmmoReserves = count;
        UpdateReloadableAppearance((ent.Owner, ent.Comp));
        Dirty(ent);
        return true;
    }

    public void UpdateReloadableAppearance(Entity<StellarGunReloadableComponent> ent)
    {
        if (!Timing.IsFirstTimePredicted || !TryComp<AppearanceComponent>(ent, out var appearance))
            return;

        _appearance.SetData(ent, AmmoVisuals.HasAmmo, ent.Comp.AmmoCount != 0, appearance);
        _appearance.SetData(ent, AmmoVisuals.AmmoCount, ent.Comp.AmmoCount ?? int.MaxValue, appearance);
        _appearance.SetData(ent, AmmoVisuals.AmmoMax, ent.Comp.AmmoMagCapacity ?? int.MaxValue, appearance);
    }

    #region Examines
    private void OnReloadableExamined(Entity<StellarGunReloadableComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.ShowExamineText || ent.Comp.AmmoName is null || ent.Comp.AmmoSuffix is null || ent.Comp.AmmoCount is null || ent.Comp.AmmoReserves is null)
            return;

        if (ent.Comp.ShowWeaponType)
            args.PushMarkup(Loc.GetString("stellar-weapon-type-examine", ("ammo", Loc.GetString(ent.Comp.AmmoName)), ("suffix", Loc.GetString(ent.Comp.AmmoSuffix))));

        args.PushMarkup(Loc.GetString("stellar-reloadable-ammo-examine", ("count", ent.Comp.AmmoCount.Value + ent.Comp.AmmoReserves.Value)));
    }

    private void OnAmmoExamined(Entity<StellarAmmoComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.ShowExamineText || ent.Comp.AmmoName is null || ent.Comp.AmmoSuffix is null || ent.Comp.CurrentAmmo is null)
            return;

        if (ent.Comp.ShowWeaponType)
            args.PushMarkup(Loc.GetString("stellar-ammo-type-examine", ("ammo", Loc.GetString(ent.Comp.AmmoName)), ("suffix", Loc.GetString(ent.Comp.AmmoSuffix))));

        if (ent.Comp.InfiniteAmmo)
            return;

        args.PushMarkup(Loc.GetString("stellar-ammo-remaining-examine", ("count", ent.Comp.CurrentAmmo.Value), ("suffix", Loc.GetString(ent.Comp.AmmoSuffix, ("count", ent.Comp.CurrentAmmo.Value))))); // New bracket world record
    }

    private void OnRegenExamined(Entity<StellarAmmoRegenComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.ShowExamineText)
            return;

        args.PushMarkup(Loc.GetString("stellar-ammo-regen-examine", ("ammo", ent.Comp.AmmoRegenerated), ("count", ent.Comp.RegenInterval)));
    }
    #endregion
}

[Serializable, NetSerializable]
public sealed partial class StellarAmmoSourceDoAfter : SimpleDoAfterEvent;
