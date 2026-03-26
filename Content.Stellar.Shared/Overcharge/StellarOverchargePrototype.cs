// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Overcharge;

[Prototype]
public sealed partial class StellarOverchargePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public LocId AnnouncementText;

    [DataField(required: true)]
    public LocId AnnouncementTextHyper;
}
