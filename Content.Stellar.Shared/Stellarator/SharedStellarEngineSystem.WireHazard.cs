// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.Damage.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Stellarator;

public abstract partial class SharedStellarEngineSystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;

    private static readonly EntProtoId BlueSparks = "StellarEffectSparksBlue";

    private void OnWireStepAttempt(Entity<StellarEngineWireComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnWireStepped(Entity<StellarEngineWireComponent> ent, ref StepTriggeredOffEvent args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var epicenter = _transform.GetMapCoordinates(ent).Position;
        var targetPos = _transform.GetWorldPosition(args.Tripper);
        var direction = targetPos - epicenter;

        if (direction == Vector2.Zero) // what??
            return;

        var throwDirection = direction.Normalized() * (ent.Comp.ThrowRange - direction.Length());

        _throw.TryThrow(args.Tripper, throwDirection, Math.Abs(ent.Comp.ThrowSpeed), ent, recoil: false, compensateFriction: true);
        _damage.TryChangeDamage(args.Tripper, ent.Comp.Damage, true, true, ent);
        Sparks.DoSparks(args.Tripper, 5, BlueSparks, randomize: true);
        Audio.PlayPredicted(ent.Comp.ZapSound, ent, args.Tripper, AudioParams.Default.WithVariation(0.2f));
    }
}
