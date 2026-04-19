// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Damage;
using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Weapons;

/// <summary>
/// This component lives on the hitscans
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarGunHitscanComponent : Component
{
    [DataField] public Color? HitColor = Color.Red;

    [DataField] public Color LightColor;

    [DataField] public bool Unshaded = true;

    [DataField] public float MaxDistance = 20.0f;

    /// <summary>
    /// Minimum distance the bullet travels and retains max damage, after which falloff scaling can come into effect.
    /// </summary>
    [DataField] public float MinDistance = 2f;

    /// <summary>
    /// Modifies damage over distance. Lower values = lower damage. 1 means no falloff.
    /// </summary>
    [DataField] public float FalloffModifier = 1f;

    [DataField] public CollisionGroup CollisionMask = CollisionGroup.Opaque;

    /// <summary>
    /// How much damage the hitscan weapon will do when hitting a target.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier Damage;

    /// <summary>
    /// RSI containing the appropriate sprites for the hitscan- expecting "start", "middle", "end", and "bullet" states.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public SpriteSpecifier.Rsi Ray;
}

