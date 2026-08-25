// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Science.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause, AutoGenerateComponentState]
public sealed partial class StellarBluespaceDriveCoreComponent : Component
{
    [DataField] public DoAfterId? DoAfterId;

    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> LinkedParts = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? StartupTimer;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? AutoLeaveTimer;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan? RechargeTimer;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan RechargeStartedAt;

    /// <summary>
    /// The time it takes for the drive to end the round.
    /// </summary>
    [DataField] public TimeSpan CountdownTime = TimeSpan.FromMinutes(5);

    [DataField] public TimeSpan? ResumeTime;

    [DataField] public bool CrisisBlocked;

    [DataField] public int Progress;

    [DataField] public int TotalCrew;

    [DataField] public int DepositedEnergy;

    [DataField] public int DepositedDataDrives;

    [DataField, AutoNetworkedField] public StellarBluespaceDriveState DriveState = StellarBluespaceDriveState.Active;
}

[Serializable, NetSerializable]
public enum StellarBluespaceDriveVisuals
{
    Visuals,
}

[Serializable, NetSerializable]
public enum StellarBluespaceDriveState
{
    Active,
    Charging,
    Spooling,
    Ready,
}
