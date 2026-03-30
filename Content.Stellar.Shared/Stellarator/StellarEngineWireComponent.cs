// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening


using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarEngineWireComponent : Component
{
    [DataField] public bool Active;

    [DataField] public float ThrowRange;

    [DataField] public float ThrowSpeed;

    [DataField] public SoundSpecifier ZapSound = new SoundPathSpecifier("/Audio/Effects/Lightning/lightningshock.ogg");

    /// <summary>
    /// Damage dealt. Specified in YML.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = default!;
}
