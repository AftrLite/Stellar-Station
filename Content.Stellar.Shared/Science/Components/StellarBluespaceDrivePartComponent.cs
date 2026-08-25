// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Science.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StellarBluespaceDrivePartComponent : Component
{
    [DataField, AutoNetworkedField] public EntityUid? LinkedCore;

    [DataField] public SoundSpecifier? SoundCharge;
}
