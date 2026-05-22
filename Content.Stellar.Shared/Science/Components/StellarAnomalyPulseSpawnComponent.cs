// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Anomaly.Effects.Components;
using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Science.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StellarAnomalyPulseSpawnComponent : Component
{
    [DataField] public List<EntitySpawnSettingsEntry> Entries = new();

    [DataField] public bool SpawnOnSpawn = true;
}
