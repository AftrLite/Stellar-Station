// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.Science.Components;

/// <summary>
/// Component that defines an entity as a Sensor Terminal, a structure for use in Science.
/// Used for the synchronization minigame between Sensor Towers and the main Science Department.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class StellarSensorTerminalComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] [AutoPausedField]
    public TimeSpan TimeOutTimer;

    [DataField] public TimeSpan TimeOut = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField] public StellarSensorTerminalState State = StellarSensorTerminalState.Idle;

    [DataField, AutoNetworkedField] public EntityUid? PopupEffect;

    [DataField, AutoNetworkedField] public int CodeLength = 6;

    [DataField, AutoNetworkedField] public string EnteredCode = "";

    [DataField, AutoNetworkedField] public string TowerCode = string.Empty;

    [DataField, AutoNetworkedField] public EntityUid? SyncingTower;

    /// <summary>
    /// All the towers that this Terminal can synchronize with.
    /// </summary>
    [DataField, AutoNetworkedField] public HashSet<EntityUid> LinkedTowers = new();
}

[Serializable, NetSerializable]
public enum StellarSensorTerminalState
{
    Idle,
    Ringing,
    Active,
}
