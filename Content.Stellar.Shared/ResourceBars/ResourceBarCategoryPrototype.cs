// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.ResourceBars;

/// <summary>
/// A marker ID used to indicate mutually exclusive resource bars
/// </summary>
[Prototype]
public sealed partial class ResourceBarCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
