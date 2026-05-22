// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Animations;

/// <summary>
/// Marker component for entities who should be automatically deleted when they complete an animation.
/// Useful for visual effects.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StellarDeleteOnAnimateComponent : Component
{
    [DataField] public StellarDeleteOnAnimateMethod Method = StellarDeleteOnAnimateMethod.OnAnyFinish;

    [DataField] public string? AnimateKey;
}

[Serializable, NetSerializable]
public enum StellarDeleteOnAnimateMethod
{
    OnKeyStop,
    OnKeyFinish,
    OnAnyStop,
    OnAnyFinish,
}
