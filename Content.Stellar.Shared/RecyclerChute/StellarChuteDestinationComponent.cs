// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-CosmicCult

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Stellar.Shared.RecyclerChute;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class StellarChuteDestinationComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))] [AutoPausedField]
    public TimeSpan CooldownTimer;

    [DataField] public TimeSpan Cooldown = TimeSpan.FromSeconds(3);

    [DataField] public EntProtoId ArrivalVfx = "StellarVfxGenericSmokePulse";

    [DataField] public SoundSpecifier? SoundArrive = new SoundPathSpecifier("/Audio/_ST/Machines/chute-destination.ogg");
}
