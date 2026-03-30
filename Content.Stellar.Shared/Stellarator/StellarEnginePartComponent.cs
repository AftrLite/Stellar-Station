// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Stellarator;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedStellarEngineSystem))]
public sealed partial class StellarEnginePartComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedCore;
}
