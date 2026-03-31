// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Stellar.Client.Lights;

public sealed class StellarRayCastPointLightSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarRayCastPointLightComponent, ComponentShutdown>(OnShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StellarRayCastPointLightComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            SharedPointLightComponent? pointLight = null;
            if (!_pointLight.ResolveLight(uid, ref pointLight))
                continue;

            if (pointLight.ContainerOccluded || !pointLight.Enabled)
            {
                if (comp.SpawnedLight is { } light)
                {
                    QueueDel(light);
                    comp.SpawnedLight = null;
                }
                continue;
            }

            if (comp.SpawnedLight is null)
                comp.SpawnedLight = Spawn(comp.LightPrototype);

            var (position, rotation) = _transform.GetWorldPositionRotation(xform);

            var ray = new CollisionRay(position, rotation.ToWorldVec(), (int) comp.CollisionMask);
            var rayCastResults = _physics.IntersectRay(xform.MapID, ray, comp.Distance);

            MapCoordinates? targetPosition = null;
            foreach (var result in rayCastResults)
            {
                targetPosition = new MapCoordinates(result.HitPos, xform.MapID);
                break;
            }

            if (targetPosition is null)
                targetPosition = new MapCoordinates(position + rotation.ToWorldVec() * comp.Distance, xform.MapID);

            var oldPosition = _transform.GetMapCoordinates(comp.SpawnedLight.Value);
            if (oldPosition.MapId == targetPosition.Value.MapId)
                targetPosition = new(Vector2.Lerp(oldPosition.Position, targetPosition.Value.Position, frameTime), xform.MapID);

            _transform.SetMapCoordinates(comp.SpawnedLight.Value, targetPosition.Value);
        }
    }

    private void OnShutdown(Entity<StellarRayCastPointLightComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.SpawnedLight is { } light)
            QueueDel(light);
    }

}
