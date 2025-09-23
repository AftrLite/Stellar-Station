using Content.Shared.Movement.Systems;
using Content.Shared._ST.Movement;

namespace Content.Server._ST.Movement;

public sealed class StellarSprintSystem : SharedStellarSprintSystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StellarSprintComponent, MapInitEvent>(OnMapInit);
    }
    private void OnMapInit(Entity<StellarSprintComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Energy = ent.Comp.EnergyMax;
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }
}
