// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared._ES.Camera;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Throwing;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Content.Stellar.Shared.Weapons;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Stellar.Server.Science;

public sealed class StellarSciencePortablesSystem : SharedStellarSciencePortablesSystem
{
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;
    [Dependency] private readonly SharedStellarGunSystem _gun = default!;
    [Dependency] private readonly SharedStellarScienceAnomalySystem _anom = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSciencePortablesComponent, StellarAbeBeamDoAfter>(OnBeam);
        SubscribeLocalEvent<StellarSciencePortablesComponent, StellarBeaHarvestDoAfter>(OnHarvest);
    }

    private void OnBeam(Entity<StellarSciencePortablesComponent> ent, ref StellarAbeBeamDoAfter args)
    {
        if (args.Handled || args.Cancelled)
            return;

        _gun.ConstructHitscan(ent, args.Beam, Transform(ent).Coordinates, args.MapDirection.Normalized(), args.Muzzle, true);
        _shake.LerpedShake(ent, 0.75f, 0.75f, 0.0085f, 20f);
        Audio.PlayPvs(ent.Comp.SoundShoot, ent);
        ent.Comp.DoAfterId = null;
        args.Handled = true;
        Dirty(ent);
    }

    private void OnHarvest(Entity<StellarSciencePortablesComponent> ent, ref StellarBeaHarvestDoAfter args)
    {
        if (args.Handled || args.Cancelled || !TryComp<StellarAnomalyComponent>(args.Target, out var anomComp) || anomComp.IntegrityPipsValue is null)
        {
            Ambience.SetAmbience(ent, false);
            return;
        }

        var target = args.Target.Value;
        var ev1 = new StellarAnomalyDecrementEvent();
        var resource = Random.Prob((anomComp.IntegrityPipsValue.Value - 1) * 0.1f) ? anomComp.HarvestOutputRare : anomComp.HarvestOutput; // %chance to harvest rare resources scales with remaining integrity. E.g. higher integrity = better odds. This also makes powerful anomalies more valuable.
        var pulseChance = Math.Clamp(2f / (float) anomComp.IntegrityPipsValue, 0f, 1f); // Pulse chance scales off of Integrity. The less integrity an anomaly has, the higher its chance to pulse when Harvested. This formula makes Anomalies guaranteed to pulse if you harvest them while "Fading".
        var output = Spawn(resource, Transform(ent).Coordinates);

        if (Random.Prob(pulseChance))
            _anom.MakeAnomalyPulse((target, anomComp));

        _throw.TryThrow(output, new EntityCoordinates(ent, new Vector2(0, 2)));
        _shake.LerpedShake(target, 0.75f, 0.75f, 0.0085f, 20f);
        Ambience.SetAmbience(ent, false);
        anomComp.HarvestDoAfterId = null;
        ent.Comp.DoAfterId = null;
        args.Handled = true;

        Spawn(ent.Comp.HarvestVfx, Transform(target).Coordinates);
        RaiseLocalEvent(target, ref ev1);
        Dirty(ent);
    }

    protected override void OnPortablesMenu(Entity<StellarSciencePortablesComponent> ent, ref StellarSciRadialMessage args)
    {
        base.OnPortablesMenu(ent, ref args);
        switch (args.Method)
        {
            case SciPortableMenuMethod.Deploy:
                if (Transform(ent).Anchored)
                {
                    TransformSystem.Unanchor(ent);
                    Appearance.SetData(ent, StellarPortablesVisuals.Visuals, StellarPortablesState.Undeploying);
                }
                else
                {
                    TransformSystem.AnchorEntity(ent);
                    if (ent.Comp.PortablesType == StellarPortablesType.Abe)
                        TransformSystem.SetLocalRotation(ent.Owner, Angle.Zero + Angle.FromDegrees(90));

                    Appearance.SetData(ent, StellarPortablesVisuals.Visuals, StellarPortablesState.Deploying);
                    if (TryComp<PullableComponent>(ent, out var pullable))
                        Pull.TryStopPull(ent, pullable);
                }
                break;
        }
    }
}
