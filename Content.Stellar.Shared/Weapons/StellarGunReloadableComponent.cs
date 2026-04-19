// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
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
public sealed partial class StellarGunReloadableComponent : Component
{
    [DataField, AutoNetworkedField] public LocId? AmmoName;

    [DataField, AutoNetworkedField] public LocId? AmmoSuffix;

    [DataField(required: true)] public ProtoId<StellarGunTypePrototype>? WeaponType;

    [DataField] public StellarGunMethod ShootingMethod = StellarGunMethod.Hitscan;

    [DataField] public Angle MultiShotSpread = Angle.FromDegrees(5);

    [DataField] public Angle MultiShotWiggleMin = Angle.FromDegrees(-0.5);

    [DataField] public Angle MultiShotWiggleMax = Angle.FromDegrees(0.5);

    [DataField] public TimeSpan ReloadTime = TimeSpan.FromSeconds(1);

    [DataField] public DoAfterId? ReloadDoAfter;

    [DataField] public EntProtoId? MuzzleFlash;

    [DataField] public float? MaxGunRange;

    [DataField] public float? RampingFireRate;

    [DataField] public float RampingBulletsNeeded = 1;

    [DataField] public int MultiShotAmount = 1;

    [DataField, AutoNetworkedField] public bool ShowExamineText = true;

    [DataField, AutoNetworkedField] public bool ShowWeaponType = true;

    [DataField] public bool ModulatePitch = true;

    /// <summary>
    /// The prototype ID of the entity that this gun shoots.
    /// </summary>
    [DataField] public EntProtoId Shootable;

    /// <summary>
    /// Max ammo reserves.
    /// </summary>
    [DataField] [AutoNetworkedField] public int? AmmoMaxReserves;

    /// <summary>
    /// Current ammo reserves.
    /// </summary>
    [DataField] [AutoNetworkedField] public int? AmmoReserves;

    /// <summary>
    /// Max ammo "magazine" capacity.
    /// </summary>
    [DataField] [AutoNetworkedField] public int? AmmoMagCapacity;

    /// <summary>
    /// Current ammo left. Initialized to capacity unless they are non-null and differ.
    /// </summary>
    [DataField] [AutoNetworkedField] public int? AmmoCount;

    /// <summary>
    /// A static amount of ammo to reload per reload. This also makes the weapon repeatedly reload until its magcapacity is filled. Used for shotguns.
    /// </summary>
    [DataField] [AutoNetworkedField] public int? AmmoPerReload;

    /// <summary>
    /// The entity hosting the audio playback for reload sounds. Used for cancelling reload sounds when the Reload DoAfter is cancelled.
    /// </summary>
    [DataField] public EntityUid? ReloadAudioStream;

    /// <summary>
    /// The sound that plays when attempting to shoot but no ammo is available.
    /// </summary>
    [DataField] public SoundSpecifier? SoundEmpty = new SoundPathSpecifier("/Audio/Weapons/Guns/Empty/empty.ogg");

    /// <summary>
    /// The sound that plays in addition to the regular firing sound when the last count of ammo is fired.
    /// </summary>
    [DataField] public SoundSpecifier? SoundLast;

    /// <summary>
    /// The sound that plays when this weapon is reloaded.
    /// </summary>
    [DataField] public SoundSpecifier? SoundReload;

    /// <summary>
    /// A sprite used to represent the ammo in the UI
    /// </summary>
    [DataField] public SpriteSpecifier? AmmoIcon;
}
[Serializable, NetSerializable]
public enum StellarGunMethod : byte
{
    Hitscan,
    Projectile,
}
