// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Weapons;

[Prototype]
public sealed partial class StellarGunTypePrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)] public LocId Ammo;

    [DataField(required: true)] public LocId Suffix;
}
