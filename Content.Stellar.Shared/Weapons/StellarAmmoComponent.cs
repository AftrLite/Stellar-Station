// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Weapons;

/// <summary>
/// Component that designates an item as Ammo for a Stellar Gun Type. E.g. When used on an item with StellarGunTypesReloadable, the item is reloaded, potentially consuming the ammo item in the process.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarAmmoComponent : Component
{
    public DoAfterId? DoAfterId = null;

    [DataField, AutoNetworkedField] public LocId? AmmoName;

    [DataField, AutoNetworkedField] public LocId? AmmoSuffix;

    [DataField(required: true)] public ProtoId<StellarGunTypePrototype>? WeaponType;

    /// <summary>
    /// Designates what behaviour to use for entities with this component. Ammo is for Items, Reloader is for Structures (e.g. an Ammo Dispenser you use a Gun on to refill it)
    /// </summary>
    [DataField(required: true), AutoNetworkedField] public StellarAmmoBehaviour Behaviour = StellarAmmoBehaviour.Ammo;

    [DataField] public SoundSpecifier? AmmoSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/bullet_insert.ogg");

    [DataField, AutoNetworkedField] public bool ShowExamineText = true;

    [DataField, AutoNetworkedField] public bool ShowWeaponType = true;

    [DataField, AutoNetworkedField] public bool UsesDoAfter;

    [DataField, AutoNetworkedField] public int? CurrentAmmo;

    [DataField, AutoNetworkedField] public int? MaxAmmo;

    [DataField] public bool InfiniteAmmo;

    [DataField] public int AmmoPerDoAfter = 2;

    [DataField] public TimeSpan DoAfterTime = TimeSpan.FromSeconds(0.5);

    public readonly int MinAmmo = 0; // If you wanted to modify this you're silly and stinky. Prithee wander elsewhere
}

[Serializable, NetSerializable]
public enum StellarAmmoBehaviour
{
    Ammo,
    Reloader,
}
