// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Dataset;
using Content.Shared.DoAfter;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science.Components;

/// <summary>
/// Component that defines an entity as an A.P.E. Used for Anomaly containment.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarAbeComponent : Component
{
    [DataField] public CollisionGroup CollisionMask;

    [DataField] public float BeamRange = 7;

    [DataField] public Color BeamColor = Color.Orange;

    [DataField] public TimeSpan BeamChargeTime = TimeSpan.FromSeconds(2.15);

    [DataField] public TimeSpan TransformTime = TimeSpan.FromSeconds(0.7);

    [DataField] public DoAfterId? DoAfterId;

    [DataField] public SoundSpecifier? SoundCharge;

    [DataField] public SoundSpecifier? SoundShoot;

    [DataField] public SoundSpecifier? SoundDeploy;

    [DataField] public SoundSpecifier? SoundUndeploy;

    [DataField] public float DialogueChance = 0.1f;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? DialogueStabilized;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? DialogueUndeployed;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? DialogueDeployed;
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
