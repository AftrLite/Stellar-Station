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
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popUp = default!;

    private void InitializeTypes()
    {
        base.Initialize();
        SubscribeLocalEvent<StellarGunTypesAmmoComponent, InteractUsingEvent>(OnAmmoInteractUsing);
        SubscribeLocalEvent<StellarGunTypesAmmoComponent, MapInitEvent>(OnAmmoInit);
        SubscribeLocalEvent<StellarGunTypesAmmoComponent, ExaminedEvent>(OnAmmoExamined);

        SubscribeLocalEvent<StellarGunTypesReloadableComponent, MapInitEvent>(OnReloadableInit);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, StellarAmmoSourceDoAfter>(OnAmmoSourceDoAfter);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, InteractUsingEvent>(OnReloadableInteractUsing);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, GetAmmoCountEvent>(OnReloadableAmmoCount);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, ExaminedEvent>(OnReloadableExamined);
    }

    private void OnReloadableInit(Entity<StellarGunTypesReloadableComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.AmmoName is null && _proto.TryIndex(ent.Comp.WeaponType, out var proto))
        {
            ent.Comp.AmmoName = proto.Ammo;
            ent.Comp.AmmoSuffix = proto.Suffix;
        }

        if (ent.Comp.AmmoCount is null)
            ent.Comp.AmmoCount = ent.Comp.AmmoCapacity;

        Dirty(ent);
        UpdateReloadableAppearance(ent);
    }

    private void OnAmmoInit(Entity<StellarGunTypesAmmoComponent> ent, ref MapInitEvent args)
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

    private void OnAmmoSourceDoAfter(Entity<StellarGunTypesReloadableComponent> ent, ref StellarAmmoSourceDoAfter args)
    {
        if (args.Handled || args.Cancelled || args.Used is null)
            return;

        if (!TryComp<StellarGunTypesAmmoComponent>(args.Used, out var ammoSource)
            || ent.Comp.WeaponType != ammoSource.WeaponType)
            return;

        var handled = TryTransferAmmo(args.User, ammoSource.AmmoPerDoAfter, (args.Used.Value, ammoSource), ent);
        args.Handled = handled;
        if (ent.Comp.AmmoCount != ent.Comp.AmmoCapacity && ammoSource.CurrentAmmo > ammoSource.MinAmmo)
            args.Repeat = handled;
    }

    private void OnReloadableInteractUsing(Entity<StellarGunTypesReloadableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(ent) || TerminatingOrDeleted(args.Used))
            return;

        if (!TryComp<StellarGunTypesAmmoComponent>(args.Used, out var ammoSource)
            || ent.Comp.WeaponType != ammoSource.WeaponType
            || ent.Comp.AmmoCapacity == ent.Comp.AmmoCount)
            return;

        if (ammoSource.UsesDoAfter)
        {
            args.Handled = TryAmmoDoAfter(args.User, (args.Used, ammoSource), ent);
            return;
        }

        args.Handled = TryTransferAmmo(args.User, (args.Used, ammoSource), ent);
    }

    private void OnAmmoInteractUsing(Entity<StellarGunTypesAmmoComponent> ammoTarget, ref InteractUsingEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(ammoTarget) || TerminatingOrDeleted(args.Used))
            return;

        if (ammoTarget.Comp.Behaviour == StellarAmmoBehaviour.Reloader
            && TryComp<StellarGunTypesReloadableComponent>(args.Used, out var weaponSource)
            && ammoTarget.Comp.WeaponType == weaponSource.WeaponType
            && weaponSource.AmmoCapacity != weaponSource.AmmoCount)
        {
            if (ammoTarget.Comp.UsesDoAfter)
            {
                args.Handled = TryAmmoDoAfter(args.User, args.Used, ammoTarget);
                return;
            }
            args.Handled = TryTransferAmmo(args.User, ammoTarget, (args.Used, weaponSource));
            return;
        }

        if (TryComp<StellarGunTypesAmmoComponent>(args.Used, out var ammoSource) && ammoTarget.Comp.WeaponType == ammoSource.WeaponType)
            args.Handled = TryTransferAmmo(args.User, (args.Used, ammoSource), ammoTarget);
    }

    private bool TryAmmoDoAfter(EntityUid user, Entity<StellarGunTypesAmmoComponent> ammoSource, Entity<StellarGunTypesReloadableComponent> ammoTarget)
    {
        if (_doAfter.IsRunning(ammoSource.Comp.DoAfterId) && !_netManager.IsClient)
        {
            _popUp.PopupEntity(Loc.GetString("stellar-ammo-reloader-occupied"), user, user, PopupType.SmallCaution);
            return false;
        }
        if (!_doAfter.IsRunning(ammoSource.Comp.DoAfterId))
        {
            var doArgs = new DoAfterArgs(EntityManager, user, ammoSource.Comp.DoAfterTime, new StellarAmmoSourceDoAfter(), ammoTarget, user, ammoSource)
            {
                MovementThreshold = 0.15f,
                DistanceThreshold = 0.25f,
                BreakOnHandChange = true,
                BreakOnDropItem = true,
            };
            var handled = _doAfter.TryStartDoAfter(doArgs, out var doAfterId);
            ammoSource.Comp.DoAfterId = doAfterId;
            return handled;
        }
        return false;
    }

    private bool TryAmmoDoAfter(EntityUid user, EntityUid used, Entity<StellarGunTypesAmmoComponent> ammoSource)
    {
        if (_doAfter.IsRunning(ammoSource.Comp.DoAfterId) && !_netManager.IsClient)
        {
            _popUp.PopupEntity(Loc.GetString("stellar-ammo-reloader-occupied"), user, user, PopupType.SmallCaution);
            return false;
        }
        if (!_doAfter.IsRunning(ammoSource.Comp.DoAfterId))
        {
            var doArgs = new DoAfterArgs(EntityManager, user, ammoSource.Comp.DoAfterTime, new StellarAmmoSourceDoAfter(), used, user, ammoSource)
            {
                MovementThreshold = 0.15f,
                DistanceThreshold = 0.25f,
                BreakOnHandChange = true,
                BreakOnDropItem = true,
            };
            var handled = _doAfter.TryStartDoAfter(doArgs, out var doAfterId);
            ammoSource.Comp.DoAfterId = doAfterId;
            return handled;
        }
        return false;
    }

    private bool TryTransferAmmo(EntityUid user, int amount, Entity<StellarGunTypesAmmoComponent> ammoSource, Entity<StellarGunTypesReloadableComponent> ammoTarget)
    {
        if (ammoTarget.Comp.AmmoCapacity is null
            || ammoTarget.Comp.AmmoCount is null
            || ammoTarget.Comp.AmmoCount == ammoTarget.Comp.AmmoCapacity
            || ammoSource.Comp.CurrentAmmo is null)
            return false;

        var weaponAmmoDiff = ammoTarget.Comp.AmmoCapacity.Value - ammoTarget.Comp.AmmoCount.Value;
        var weaponAmmoToTransfer = Math.Clamp(weaponAmmoDiff, 0, amount);

        if (!ModifyAmmoCount(ammoTarget.Owner, weaponAmmoToTransfer))
            return false;

        if (!ammoSource.Comp.InfiniteAmmo)
            ammoSource.Comp.CurrentAmmo -= weaponAmmoToTransfer;

        Dirty(ammoSource);
        _audio.PlayPredicted(ammoSource.Comp.AmmoSound, user, user);
        if (ammoSource.Comp.CurrentAmmo <= 0)
            PredictedQueueDel(ammoSource);
        return true;
    }

    private bool TryTransferAmmo(EntityUid user, Entity<StellarGunTypesAmmoComponent> ammoSource, Entity<StellarGunTypesReloadableComponent> ammoTarget)
    {
        if (ammoTarget.Comp.AmmoCapacity is null
            || ammoTarget.Comp.AmmoCount is null
            || ammoTarget.Comp.AmmoCount == ammoTarget.Comp.AmmoCapacity
            || ammoSource.Comp.CurrentAmmo is null)
            return false;

        var weaponAmmoDiff = ammoTarget.Comp.AmmoCapacity.Value - ammoTarget.Comp.AmmoCount.Value;
        var weaponAmmoToTransfer = Math.Clamp(weaponAmmoDiff, 0, ammoSource.Comp.CurrentAmmo.Value);

        if (!ModifyAmmoCount(ammoTarget.Owner, weaponAmmoToTransfer))
            return false;

        if (!ammoSource.Comp.InfiniteAmmo)
            ammoSource.Comp.CurrentAmmo -= weaponAmmoToTransfer;

        Dirty(ammoSource);
        _audio.PlayPredicted(ammoSource.Comp.AmmoSound, user, user);
        if (ammoSource.Comp.CurrentAmmo <= 0)
            PredictedQueueDel(ammoSource);

        return true;
    }

    private bool TryTransferAmmo(EntityUid user, Entity<StellarGunTypesAmmoComponent> ammoSource, Entity<StellarGunTypesAmmoComponent> ammoTarget)
    {
        if (ammoTarget.Comp.CurrentAmmo is null || ammoTarget.Comp.MaxAmmo is null || ammoSource.Comp.CurrentAmmo is null || ammoSource.Comp.MaxAmmo is null)
            return false;

        switch (ammoTarget.Comp.Behaviour) // You'd think there'd be a smarter way of handling this, right? But i can't think of one!
        {
            case StellarAmmoBehaviour.Ammo:
                var ammoDiff = ammoTarget.Comp.MaxAmmo.Value - ammoTarget.Comp.CurrentAmmo.Value;
                var ammoToTransfer = Math.Clamp(ammoDiff, 0, ammoSource.Comp.CurrentAmmo.Value);

                if (ammoTarget.Comp.CurrentAmmo == ammoTarget.Comp.MaxAmmo)
                    return false;

                if (!ammoSource.Comp.InfiniteAmmo)
                    ammoSource.Comp.CurrentAmmo -= ammoToTransfer;
                ammoTarget.Comp.CurrentAmmo += ammoToTransfer;

                Dirty(ammoTarget);
                Dirty(ammoSource);
                _audio.PlayPredicted(ammoSource.Comp.AmmoSound, user, user);
                if (ammoSource.Comp.CurrentAmmo <= 0)
                    PredictedQueueDel(ammoSource);
                return true;

            case StellarAmmoBehaviour.Reloader:
                var reloaderAmmoDiff = ammoSource.Comp.MaxAmmo.Value - ammoSource.Comp.CurrentAmmo.Value;
                var reloaderAmmoToTransfer = Math.Clamp(reloaderAmmoDiff, 0, ammoTarget.Comp.CurrentAmmo.Value);

                if (ammoSource.Comp.CurrentAmmo == ammoSource.Comp.MaxAmmo)
                    return false;

                if (!ammoTarget.Comp.InfiniteAmmo)
                    ammoTarget.Comp.CurrentAmmo -= reloaderAmmoToTransfer;
                ammoSource.Comp.CurrentAmmo += reloaderAmmoToTransfer;

                Dirty(ammoTarget);
                Dirty(ammoSource);
                _audio.PlayPredicted(ammoTarget.Comp.AmmoSound, user, user); // Play the gun's reload sound rather than the Ammo's.
                return true; // don't QueueDel because reloaders shouldn't be destroyed by drawing ammo from them.
        }
        return false;
    }

    private void OnReloadableAmmoCount(Entity<StellarGunTypesReloadableComponent> ent, ref GetAmmoCountEvent args)
    {
        args.Capacity = ent.Comp.AmmoCapacity ?? int.MaxValue;
        args.Count = ent.Comp.AmmoCount ?? int.MaxValue;
    }

    private bool ModifyAmmoCount(Entity<StellarGunTypesReloadableComponent?> ent, int delta)
    {
        if (!Resolve(ent, ref ent.Comp, false) || ent.Comp.AmmoCount == null)
            return false;

        return UpdateReloadableCount((ent.Owner, ent.Comp), ent.Comp.AmmoCount.Value + delta);
    }

    private bool UpdateReloadableCount(Entity<StellarGunTypesReloadableComponent?> ent, int count)
    {
        if (!Resolve(ent, ref ent.Comp, false) || count > ent.Comp.AmmoCapacity)
            return false;

        ent.Comp.AmmoCount = count;
        UpdateReloadableAppearance((ent.Owner, ent.Comp));
        Dirty(ent);
        return true;
    }

    private void OnReloadableExamined(Entity<StellarGunTypesReloadableComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.ShowExamineText || ent.Comp.AmmoName is null || ent.Comp.AmmoSuffix is null || ent.Comp.AmmoCount is null)
            return;

        if (ent.Comp.ShowWeaponType)
            args.PushMarkup(Loc.GetString("stellar-weapon-type-examine", ("ammo", Loc.GetString(ent.Comp.AmmoName)), ("suffix", Loc.GetString(ent.Comp.AmmoSuffix))));

        args.PushMarkup(Loc.GetString("stellar-reloadable-ammo-examine", ("count", ent.Comp.AmmoCount.Value)));
    }

    private void OnAmmoExamined(Entity<StellarGunTypesAmmoComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.ShowExamineText || ent.Comp.AmmoName is null || ent.Comp.AmmoSuffix is null || ent.Comp.CurrentAmmo is null)
            return;

        if (ent.Comp.ShowWeaponType)
            args.PushMarkup(Loc.GetString("stellar-ammo-type-examine", ("ammo", Loc.GetString(ent.Comp.AmmoName)), ("suffix", Loc.GetString(ent.Comp.AmmoSuffix))));

        if (ent.Comp.InfiniteAmmo)
            return;

        args.PushMarkup(Loc.GetString("stellar-ammo-remaining-examine", ("count", ent.Comp.CurrentAmmo.Value), ("suffix", Loc.GetString(ent.Comp.AmmoSuffix, ("count", ent.Comp.CurrentAmmo.Value))))); // New bracket world record
    }

    public void UpdateReloadableAppearance(Entity<StellarGunTypesReloadableComponent> ent)
    {
        if (!_timing.IsFirstTimePredicted || !TryComp<AppearanceComponent>(ent, out var appearance))
            return;

        _appearance.SetData(ent, AmmoVisuals.HasAmmo, ent.Comp.AmmoCount != 0, appearance);
        _appearance.SetData(ent, AmmoVisuals.AmmoCount, ent.Comp.AmmoCount ?? int.MaxValue, appearance);
        _appearance.SetData(ent, AmmoVisuals.AmmoMax, ent.Comp.AmmoCapacity ?? int.MaxValue, appearance);
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarAmmoSourceDoAfter : SimpleDoAfterEvent;
