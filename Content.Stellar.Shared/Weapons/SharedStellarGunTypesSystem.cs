// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Stellar.Shared.Weapons;

public sealed class SharedStellarGunTypesSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedPopupSystem _popUp = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarGunTypesReloadableComponent, StellarAmmoSourceDoAfter>(OnAmmoSourceDoAfter);
        SubscribeLocalEvent<StellarGunTypesReloadableComponent, InteractUsingEvent>(OnReloadableInteractUsing);
        SubscribeLocalEvent<StellarGunTypesAmmoComponent, InteractUsingEvent>(OnAmmoInteractUsing);

        SubscribeLocalEvent<StellarGunTypesReloadableComponent, MapInitEvent>(OnReloadableInit);
        SubscribeLocalEvent<StellarGunTypesAmmoComponent, MapInitEvent>(OnAmmoInit);

        SubscribeLocalEvent<StellarGunTypesReloadableComponent, ExaminedEvent>(OnReloadableExamined);
        SubscribeLocalEvent<StellarGunTypesAmmoComponent, ExaminedEvent>(OnAmmoExamined);
    }

    private void OnReloadableInit(Entity<StellarGunTypesReloadableComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.AmmoName is null && _proto.TryIndex(ent.Comp.WeaponType, out var proto))
        {
            ent.Comp.AmmoName = proto.Ammo;
            ent.Comp.AmmoSuffix = proto.Suffix;
        }  // if we're not overriding in YML, set it to the gunType's preset name and suffix.

        Dirty(ent);
    }

    private void OnAmmoInit(Entity<StellarGunTypesAmmoComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.AmmoName is null && _proto.TryIndex(ent.Comp.WeaponType, out var proto))
        {
            ent.Comp.AmmoName = proto.Ammo;
            ent.Comp.AmmoSuffix = proto.Suffix;
        }  // if we're not overriding in YML, set it to the gunType's preset name and suffix.

        if (ent.Comp.CurrentAmmo is null) // If the currentAmmo isn't set, init it to the max ammo.
            ent.Comp.CurrentAmmo = ent.Comp.MaxAmmo;

        Dirty(ent);
    }

    private void OnAmmoSourceDoAfter(Entity<StellarGunTypesReloadableComponent> ent, ref StellarAmmoSourceDoAfter args)
    {
        if (args.Handled || args.Cancelled || args.Used is null)
            return;

        if (!TryComp<BasicEntityAmmoProviderComponent>(ent, out var ammoTarget)
            || !TryComp<StellarGunTypesAmmoComponent>(args.Used, out var ammoSource)
            || ent.Comp.WeaponType != ammoSource.WeaponType)
            return;

        var handled = TryTransferAmmo(args.User, ammoSource.AmmoPerDoAfter, (args.Used.Value, ammoSource), (ent, ammoTarget));
        args.Handled = handled;
        if (ammoTarget.Count != ammoTarget.Capacity && ammoSource.CurrentAmmo > ammoSource.MinAmmo)
            args.Repeat = handled;
    }

    private void OnReloadableInteractUsing(Entity<StellarGunTypesReloadableComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(ent) || TerminatingOrDeleted(args.Used))
            return;

        if (!TryComp<BasicEntityAmmoProviderComponent>(ent, out var ammoTarget)
            || !TryComp<StellarGunTypesAmmoComponent>(args.Used, out var ammoSource)
            || ent.Comp.WeaponType != ammoSource.WeaponType
            || ammoTarget.Capacity == ammoTarget.Count)
            return;

        if (ammoSource.UsesDoAfter)
        {
            args.Handled = TryAmmoDoAfter(args.User, (args.Used, ammoSource), ent);
            return;
        }

        args.Handled = TryTransferAmmo(args.User, (args.Used, ammoSource), (ent, ammoTarget));
    }

    private void OnAmmoInteractUsing(Entity<StellarGunTypesAmmoComponent> ammoTarget, ref InteractUsingEvent args)
    {
        if (args.Handled || TerminatingOrDeleted(ammoTarget) || TerminatingOrDeleted(args.Used))
            return;

        if (ammoTarget.Comp.Behaviour == StellarAmmoBehaviour.Reloader
            && TryComp<StellarGunTypesReloadableComponent>(args.Used, out var weaponSource)
            && TryComp<BasicEntityAmmoProviderComponent>(args.Used, out var ammoProvider)
            && ammoTarget.Comp.WeaponType == weaponSource.WeaponType
            && ammoProvider.Capacity != ammoProvider.Count)
        {
            if (ammoTarget.Comp.UsesDoAfter)
            {
                args.Handled = TryAmmoDoAfter(args.User, args.Used, ammoTarget);
                return;
            }
            args.Handled = TryTransferAmmo(args.User, ammoTarget, (args.Used, ammoProvider));
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

    private bool TryTransferAmmo(EntityUid user, int amount, Entity<StellarGunTypesAmmoComponent> ammoSource, Entity<BasicEntityAmmoProviderComponent> ammoTarget)
    {
        if (ammoTarget.Comp.Capacity is null
            || ammoTarget.Comp.Count is null
            || ammoTarget.Comp.Count == ammoTarget.Comp.Capacity
            || ammoSource.Comp.CurrentAmmo is null)
            return false;

        var weaponAmmoDiff = ammoTarget.Comp.Capacity.Value - ammoTarget.Comp.Count.Value;
        var weaponAmmoToTransfer = Math.Clamp(weaponAmmoDiff, 0, amount);

        if (!_gun.ChangeBasicEntityAmmoCount(ammoTarget.Owner, weaponAmmoToTransfer))
            return false;

        if (!ammoSource.Comp.InfiniteAmmo)
            ammoSource.Comp.CurrentAmmo -= weaponAmmoToTransfer;

        Dirty(ammoSource);
        _audio.PlayPredicted(ammoSource.Comp.AmmoSound, ammoSource, user);
        if (ammoSource.Comp.CurrentAmmo <= 0)
            QueueDel(ammoSource);
        return true;
    }

    private bool TryTransferAmmo(EntityUid user, Entity<StellarGunTypesAmmoComponent> ammoSource, Entity<BasicEntityAmmoProviderComponent> ammoTarget)
    {
        if (ammoTarget.Comp.Capacity is null
            || ammoTarget.Comp.Count is null
            || ammoTarget.Comp.Count == ammoTarget.Comp.Capacity
            || ammoSource.Comp.CurrentAmmo is null)
            return false;

        var weaponAmmoDiff = ammoTarget.Comp.Capacity.Value - ammoTarget.Comp.Count.Value;
        var weaponAmmoToTransfer = Math.Clamp(weaponAmmoDiff, 0, ammoSource.Comp.CurrentAmmo.Value);

        if (!_gun.ChangeBasicEntityAmmoCount(ammoTarget.Owner, weaponAmmoToTransfer))
            return false;

        if (!ammoSource.Comp.InfiniteAmmo)
            ammoSource.Comp.CurrentAmmo -= weaponAmmoToTransfer;

        Dirty(ammoSource);
        _audio.PlayPredicted(ammoSource.Comp.AmmoSound, ammoSource, user);
        if (ammoSource.Comp.CurrentAmmo <= 0)
            QueueDel(ammoSource);
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
                    QueueDel(ammoSource);
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

    private void OnReloadableExamined(Entity<StellarGunTypesReloadableComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.ShowExamineText || ent.Comp.AmmoName is null || ent.Comp.AmmoSuffix is null || !TryComp<BasicEntityAmmoProviderComponent>(ent, out var ammoProvider) || ammoProvider.Count is null)
            return;

        if (ent.Comp.ShowWeaponType)
            args.PushMarkup(Loc.GetString("stellar-weapon-type-examine", ("ammo", Loc.GetString(ent.Comp.AmmoName)), ("suffix", Loc.GetString(ent.Comp.AmmoSuffix))));

        args.PushMarkup(Loc.GetString("stellar-reloadable-ammo-examine", ("count", ammoProvider.Count.Value)));
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
}

[Serializable, NetSerializable]
public sealed partial class StellarAmmoSourceDoAfter : SimpleDoAfterEvent;
