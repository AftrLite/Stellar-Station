// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

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

    [DataField, AutoNetworkedField] public bool ShowExamineText = true;

    [DataField, AutoNetworkedField] public bool ShowWeaponType = true;
}
