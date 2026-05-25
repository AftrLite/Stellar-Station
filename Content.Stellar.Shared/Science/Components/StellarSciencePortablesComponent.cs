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
/// Component that defines an entity as "Science Portable". By default, the "portables" are the A.B.E and the B.E.A.
/// Despite their gameplay differences, they both share this component and relevant system in order to avoid needless code duplication.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarSciencePortablesComponent : Component
{
    [DataField] public CollisionGroup CollisionMask;

    [DataField] public float BeamRange = 7;

    [DataField] public Color BeamColor = Color.Orange;

    [DataField] public TimeSpan HarvestTime = TimeSpan.FromSeconds(30);

    [DataField] public TimeSpan BeamChargeTime = TimeSpan.FromSeconds(2.15);

    [DataField] public TimeSpan TransformTime = TimeSpan.FromSeconds(0.7);

    [DataField] public DoAfterId? DoAfterId;

    [DataField] public SoundSpecifier? SoundCharge;

    [DataField] public SoundSpecifier? SoundShoot;

    [DataField] public SoundSpecifier? SoundDeploy;

    [DataField] public SoundSpecifier? SoundUndeploy;

    [DataField] public float DialogueChance = 0.1f;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? DialogueHarvested;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? DialogueStabilized;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? DialogueUndeployed;

    [DataField] public ProtoId<LocalizedDatasetPrototype>? DialogueDeployed;

    [DataField] public StellarPortablesType PortablesType = StellarPortablesType.Abe;

    [DataField] public EntProtoId HarvestVfx = "StellarEffectSciencePulse";
}

[Serializable, NetSerializable]
public enum StellarPortablesType // Is this stupid? Maybe.
{
    Abe,
    Bea,
}

[Serializable, NetSerializable]
public enum StellarPortablesRadialKey
{
    Key,
}

[Serializable, NetSerializable]
public enum StellarPortablesVisuals
{
    Visuals,
}

[Serializable, NetSerializable]
public enum StellarPortablesState
{
    Undeploying,
    Undeployed,
    Deploying,
    Deployed,
}
