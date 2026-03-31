// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Client.Lights;

[RegisterComponent]
public sealed partial class StellarRayCastPointLightComponent : Component
{
    [DataField(required: true)]
    public EntProtoId LightPrototype;

    [DataField]
    public EntityUid? SpawnedLight;

    [DataField]
    public CollisionGroup CollisionMask = CollisionGroup.StellarLightImpassable;

    [DataField]
    public float Distance = 6f;
}
