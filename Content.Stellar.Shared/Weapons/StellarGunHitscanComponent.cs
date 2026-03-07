// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Weapons;

/// <summary>
/// This component lives on the hitscans
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarGunHitscanComponent : Component, IShootable
{
    [DataField] public Color? HitColor = Color.Red;

    [DataField] public Color LightColor;

    [DataField] public bool Unshaded = true;

    [DataField] public float MaxDistance = 20.0f;

    [DataField] public CollisionGroup CollisionMask = CollisionGroup.Opaque;

    /// <summary>
    /// RSI containing the appropriate sprites for the hitscan- expecting "start", "middle", "end", and "bullet" states.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public SpriteSpecifier.Rsi Ray;

    [DataField] public EntProtoId? MuzzleFlash;
}

