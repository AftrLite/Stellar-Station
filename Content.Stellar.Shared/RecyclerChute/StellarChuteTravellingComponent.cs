// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-CosmicCult

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.RecyclerChute;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class StellarChuteTravellingComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] [AutoPausedField]
    public TimeSpan ArrivalTime;

    [DataField] public TimeSpan TravelTime = TimeSpan.FromSeconds(10);
}
