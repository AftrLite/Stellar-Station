// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Weapons;

[RegisterComponent, NetworkedComponent]
public sealed partial class StellarGunHitscanComponent : Component, IShootable
{
    [DataField] public Angle MultiShotSpread = Angle.FromDegrees(5);

    [DataField] public Angle MultiShotWiggleMin = Angle.FromDegrees(-0.5);

    [DataField] public Angle MultiShotWiggleMax = Angle.FromDegrees(0.5);

    [DataField] public int MultiShotAmount = 1;

    [DataField] public Color? HitColor = Color.Red;

    [DataField] public Color LightColor;

    [DataField] public bool Unshaded = true;

    [DataField] public float MaxDistance = 20.0f;

    [DataField] public CollisionGroup CollisionMask = CollisionGroup.Opaque;

    /// <summary>
    /// RSI containing the appropriate sprites for the hitscan- expecting start, middle, end, bullet states
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public SpriteSpecifier.Rsi Ray;

    [DataField] public EntProtoId MuzzleFlash;
}

