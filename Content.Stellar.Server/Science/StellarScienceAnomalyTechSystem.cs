// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ES.Camera;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Weapons;

namespace Content.Stellar.Server.Science;

public sealed class StellarScienceAnomalyTechSystem : SharedStellarScienceAnomalyTechSystem
{
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAnomalyAbeComponent, StellarAbeBeamDoAfter>(OnAbeBeam);

        SubscribeLocalEvent<StellarRealityAnchorComponent, StellarAnchorAutoEnableDoAfter>(OnAnchorDoAfterEnable);
        SubscribeLocalEvent<StellarRealityAnchorComponent, StellarAnchorDisableDoAfter>(OnAnchorDoAfterDisable);
    }

    private void OnAbeBeam(Entity<StellarAnomalyAbeComponent> ent, ref StellarAbeBeamDoAfter args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var xform = Transform(ent);
        var beam = Spawn(args.BeamToUse);
        var hitscanEv = new HitscanTraceEvent()
        {
            FromCoordinates = xform.Coordinates, ShotDirection = args.MapDirection.Normalized(), Gun = ent, Shooter = ent,
        };
        RaiseLocalEvent(beam, ref hitscanEv);



        if (TryComp<StellarStabilizerBeamComponent>(beam, out var beamComp) && beamComp.MuzzleFlash != null)
        {
            var ev = new StellarMuzzleFlashEvent(GetNetEntity(ent), beamComp.MuzzleFlash, args.MapDirection.ToAngle());
            RaiseNetworkEvent(ev);
        }

        _shake.LerpedShake(ent, 0.75f, 0.75f, 0.0085f, 20f);
        Audio.PlayPvs(ent.Comp.SoundShoot, ent);
        ent.Comp.DoAfterId = null;
        args.Handled = true;
        Dirty(ent);
    }

    protected override void OnAbeMenu(Entity<StellarAnomalyAbeComponent> ent, ref StellarAbeRadialMessage args)
    {
        base.OnAbeMenu(ent, ref args);
        switch (args.Method)
        {
            case AbeMenuMethod.Deploy:
                if (Transform(ent).Anchored)
                {
                    TransformSystem.Unanchor(ent);
                    Appearance.SetData(ent, StellarAbeVisuals.Visuals, StellarAbeState.Undeploying);
                }
                else
                {
                    TransformSystem.AnchorEntity(ent);
                    TransformSystem.SetLocalRotation(ent.Owner, Angle.Zero + Angle.FromDegrees(90));
                    Appearance.SetData(ent, StellarAbeVisuals.Visuals, StellarAbeState.Deploying);
                    if (TryComp<PullableComponent>(ent, out var pullable))
                        Pull.TryStopPull(ent, pullable);
                }
                break;
        }
    }

    private void OnAnchorDoAfterEnable(Entity<StellarRealityAnchorComponent> ent, ref StellarAnchorAutoEnableDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        ent.Comp.Enabled = true;
    }

    private void OnAnchorDoAfterDisable(Entity<StellarRealityAnchorComponent> ent, ref StellarAnchorDisableDoAfter args)
    {
        if (args.Cancelled || args.Handled)
            return;

        ent.Comp.Enabled = false;
    }
}
