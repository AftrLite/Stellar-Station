using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._ST.Movement;

public abstract class SharedStellarSprintSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarSprintComponent, MoveInputEvent>(OnMoveInput);
        SubscribeLocalEvent<StellarSprintComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<StellarSprintComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var energyQuery = GetEntityQuery<StellarSprintComponent>();
        var query = EntityQueryEnumerator<StellarSprintActiveComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var ent, out _))
        {
            if (!energyQuery.TryGetComponent(ent, out var comp) || comp.Energy >= comp.EnergyMax)
            {
                RemComp<StellarSprintActiveComponent>(ent);
                continue;
            }

            var nextUpdate = comp.NextUpdate;
            if (nextUpdate > curTime)
                continue;
            comp.NextUpdate += TimeSpan.FromSeconds(.75f);

            if (_timing.CurTime >= comp.RegenCooldown)
                comp.Energy += GetEnergyRegen(ent, comp) * comp.RegenMod;
            if (comp.Energy > comp.EnergyMax)
                comp.Energy = comp.EnergyMax;

            if (comp.Energy <= comp.EnergyMin)
                comp.Sprinting = false;

            Math.Clamp(comp.Energy, comp.EnergyMin, comp.EnergyMax);
            comp.MoveModifier = Math.Clamp(1.5f * (comp.Energy / comp.EnergyMax) + 0.5f, 0.5f, 1f); // keb evil magic numbers. This lerps Movement Modifier between 1 and 0.5 once energy is below 33%.
            _movementSpeed.RefreshMovementSpeedModifiers(ent);
            Dirty(ent, comp);

            if (_net.IsServer)
                Log.Debug($"sprint energy: {comp.Energy}");
        }
    }

    public float GetEnergyRegen(EntityUid ent, StellarSprintComponent? comp = null)
    {
        if (!Resolve(ent, ref comp))
            return 0f;

        var curTime = _timing.CurTime;
        var pauseTime = _metadata.GetPauseTime(ent);
        return MathF.Max(0f, comp.Regen - MathF.Max(0f, (float)(curTime - (comp.NextUpdate + pauseTime)).TotalSeconds * comp.Regen));
    }

    private void OnMove(Entity<StellarSprintComponent> ent, ref MoveEvent args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var comp = ent.Comp;
        var factor = (args.NewPosition.Position - args.OldPosition.Position).Length();
        var decayAmount = comp.Decay * factor;

        if (comp.Sprinting)
        {
            comp.RegenCooldown = _timing.CurTime + comp.RegenWait;
            comp.Energy -= decayAmount * comp.DecayMod;
            if (comp.Energy < comp.EnergyMin)
                comp.Energy = comp.EnergyMin;
            EnsureComp<StellarSprintActiveComponent>(ent);
        }
    }

    private void OnMoveInput(Entity<StellarSprintComponent> ent, ref MoveInputEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if ((args.Entity.Comp.HeldMoveButtons &
             (MoveButtons.Down | MoveButtons.Left | MoveButtons.Up | MoveButtons.Right)) == 0x0)
            return;

        var sprinting = false;
        if ((args.Entity.Comp.HeldMoveButtons &
             (MoveButtons.Walk)) != 0x0)
            sprinting = true;

        var comp = ent.Comp;
        if (comp.Energy > comp.EnergyMax * comp.Resprint && sprinting && !comp.Sprinting)
            comp.Sprinting = true;

        if (!sprinting && comp.Sprinting)
            comp.Sprinting = false;
    }

    private void OnRefresh(Entity<StellarSprintComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.MoveModifier < 1)
        {
            args.ModifySpeed(1f, ent.Comp.MoveModifier);
        }
        else
            args.ModifySpeed(1f, 1f);
    }
}
