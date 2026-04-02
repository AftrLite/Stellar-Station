// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Weapons;

/// <summary>
/// Component that designates an item as a Reloadable GunType.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarGunTypesReloadableComponent : AmmoProviderComponent
{
    [DataField, AutoNetworkedField] public LocId? AmmoName;

    [DataField, AutoNetworkedField] public LocId? AmmoSuffix;

    [DataField(required: true)] public ProtoId<StellarGunTypePrototype>? WeaponType;

    [DataField] public StellarGunMethod ShootingMethod = StellarGunMethod.Hitscan;

    [DataField] public Angle MultiShotSpread = Angle.FromDegrees(5);

    [DataField] public Angle MultiShotWiggleMin = Angle.FromDegrees(-0.5);

    [DataField] public Angle MultiShotWiggleMax = Angle.FromDegrees(0.5);

    [DataField] public EntProtoId? MuzzleFlash;

    [DataField] public float? MaxGunRange;

    [DataField] public float? RampingFireRate;

    [DataField] public float RampingBulletsNeeded = 1;

    [DataField] public int MultiShotAmount = 1;

    [DataField, AutoNetworkedField] public bool ShowExamineText = true;

    [DataField, AutoNetworkedField] public bool ShowWeaponType = true;

    /// <summary>
    /// The prototype ID of the entity that this gun shoots.
    /// </summary>
    [DataField] public EntProtoId Shootable;

    /// <summary>
    /// Max ammo capacity.
    /// </summary>
    [DataField] [AutoNetworkedField] public int? AmmoCapacity;

    /// <summary>
    /// Actual ammo left. Initialized to capacity unless they are non-null and differ.
    /// </summary>
    [DataField] [AutoNetworkedField] public int? AmmoCount;

    [DataField] public SoundSpecifier? SoundEmpty = new SoundPathSpecifier("/Audio/Weapons/Guns/Empty/empty.ogg");

    /// <summary>
    /// A sprite used to represent the ammo in the UI
    /// </summary>
    [DataField] public SpriteSpecifier? Icon;
}
[Serializable, NetSerializable]
public enum StellarGunMethod : byte
{
    Hitscan,
    Projectile,
}
