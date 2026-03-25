// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;

namespace Content.Shared._ST.Shockwave;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class StellarShockwaveComponent : Component
{
    [AutoNetworkedField] public TimeSpan StartTime;

    [DataField, AutoNetworkedField] public float Duration = 0.9f;

    [DataField, AutoNetworkedField] public float VisualRange = 5f;

    [DataField, AutoNetworkedField] public float VisualWidth = 0.1f;

    [DataField, AutoNetworkedField] public float VisualForce = 0.025f; // Very touchy!

    [DataField, AutoNetworkedField] public float VisualAberration = 0.1f;
}
