// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Weapons;

/// <summary>
/// Component that designates an item as a Reloadable GunType.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarGunTypesReloadableComponent : Component
{
    [DataField, AutoNetworkedField] public LocId? AmmoName;

    [DataField, AutoNetworkedField] public LocId? AmmoSuffix;

    [DataField(required: true)] public ProtoId<StellarGunTypePrototype>? WeaponType;

    [DataField] public Angle MultiShotSpread = Angle.FromDegrees(5);

    [DataField] public Angle MultiShotWiggleMin = Angle.FromDegrees(-0.5);

    [DataField] public Angle MultiShotWiggleMax = Angle.FromDegrees(0.5);

    [DataField] public float? RampingFireRate;

    [DataField] public float RampingBulletsNeeded = 1;

    [DataField] public int MultiShotAmount = 1;

    [DataField, AutoNetworkedField] public bool ShowExamineText = true;

    [DataField, AutoNetworkedField] public bool ShowWeaponType = true;
}
