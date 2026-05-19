// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Stellar.Shared.Science;

/// <summary>
/// Component that defines an entity as an A.P.E. Used for Anomaly containment.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarAnomalyAbeComponent : Component
{

    [DataField] public CollisionGroup CollisionMask;

    [DataField] public SpriteSpecifier.Rsi BeamVisuals = new(new ResPath("/Textures/_ST/Projectiles/Guns/tachyon-heavy.rsi"), "bullet");

    [DataField] public float BeamRange = 7;

    [DataField] public Color BeamColor = Color.Orange;

    [DataField] public TimeSpan BeamChargeTime = TimeSpan.FromSeconds(2.5);

    [DataField] public TimeSpan TransformTime = TimeSpan.FromSeconds(0.7);

    [DataField] public DoAfterId? DoAfterId;

    [DataField] public SoundSpecifier? SoundCharge;

    [DataField] public SoundSpecifier? SoundShoot;

    [DataField] public SoundSpecifier? SoundDeploy;

    [DataField] public SoundSpecifier? SoundUndeploy;
}

[Serializable, NetSerializable]
public enum StellarAbeRadialKey
{
    Key,
}

[Serializable, NetSerializable]
public enum StellarAbeVisuals
{
    Visuals,
}

[Serializable, NetSerializable]
public enum StellarAbeState
{
    Undeploying,
    Undeployed,
    Deploying,
    Deployed,
}
