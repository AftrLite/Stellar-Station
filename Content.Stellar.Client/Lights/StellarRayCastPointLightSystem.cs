// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Client.Light.Components;
using Content.Client.Light.EntitySystems;
using Content.Shared.Light.Components;
using Content.Shared.Toggleable;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Stellar.Client.Lights;

public sealed class StellarRayCastPointLightSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _pointLight = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly LightBehaviorSystem _lightBehavior = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarRayCastPointLightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<StellarRayCastPointLightComponent, AppearanceChangeEvent>(OnAppearanceChange);
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
            {
                comp.SpawnedLight = Spawn(comp.LightPrototype);
                UpdateLightAppearance((uid, comp));
            }

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

    private void UpdateLightAppearance(Entity<StellarRayCastPointLightComponent> ent, AppearanceComponent? component = null)
    {
        if (ent.Comp.SpawnedLight is not { } light)
            return;

        if (!_appearance.TryGetData<bool>(ent, ToggleableVisuals.Enabled, out var enabled, component))
            return;

        if (!_appearance.TryGetData<HandheldLightPowerStates>(ent, HandheldLightVisuals.Power, out var state, component))
            return;

        if (!TryComp<LightBehaviourComponent>(light, out var lightBehaviour))
            return;

        if (_lightBehavior.HasRunningBehaviours((light, lightBehaviour)))
            _lightBehavior.StopLightBehaviour((light, lightBehaviour), resetToOriginalSettings: true);

        if (!enabled)
            return;

        switch (state)
        {
            case HandheldLightPowerStates.FullPower:
                break;
            case HandheldLightPowerStates.LowPower:
                _lightBehavior.StartLightBehaviour((light, lightBehaviour), "radiating");
                break;
            case HandheldLightPowerStates.Dying:
                _lightBehavior.StartLightBehaviour((light, lightBehaviour), "blinking");
                break;
        }
    }

    private void OnAppearanceChange(Entity<StellarRayCastPointLightComponent> ent, ref AppearanceChangeEvent args)
    {
        UpdateLightAppearance(ent, args.Component);
    }
}
