// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared._ES.Camera;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Stellar.Shared.Science;
using Content.Stellar.Shared.Science.Components;
using Content.Stellar.Shared.Weapons;

namespace Content.Stellar.Server.Science;

public sealed class StellarScienceAbeSystem : SharedStellarScienceAbeSystem
{
    [Dependency] private readonly ESScreenshakeSystem _shake = default!;
    [Dependency] private readonly SharedStellarGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarAbeComponent, StellarAbeBeamDoAfter>(OnAbeBeam);
    }

    private void OnAbeBeam(Entity<StellarAbeComponent> ent, ref StellarAbeBeamDoAfter args)
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

    protected override void OnAbeMenu(Entity<StellarAbeComponent> ent, ref StellarAbeRadialMessage args)
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
}
