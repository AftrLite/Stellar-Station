// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Weapons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class StellarAmmoRegenComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan RegenTime = default!;

    [DataField] public TimeSpan RegenInterval = TimeSpan.FromSeconds(999); // lmao

    [DataField] public bool ShowExamineText;

    /// <summary>
    /// The sound that plays when ammo is regenerated.
    /// </summary>
    [DataField] public SoundSpecifier? SoundOnRegen;

    /// <summary>
    /// The amount of ammo regenerated every time this component's regeneration occurs.
    /// </summary>
    [DataField] public int AmmoRegenerated = 1;
}
