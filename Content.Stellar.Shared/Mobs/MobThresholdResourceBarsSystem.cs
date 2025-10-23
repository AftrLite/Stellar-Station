using Content.Shared.Mobs.Systems;
using Content.Stellar.Shared.ResourceBars;

namespace Content.Stellar.Shared.Mobs;

public sealed class MobThresholdResourceBarsSystem : EntitySystem
{
    [Dependency] private readonly SharedResourceBarsSystem _resourceBars = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobThresholdResourceBarsComponent, MobThresholdChecked>(OnMobThresholdChecked);
    }

    private void OnMobThresholdChecked(Entity<MobThresholdResourceBarsComponent> ent, ref MobThresholdChecked args)
    {
        if (!_mobThreshold.TryGetNextState(ent, args.MobState.CurrentState, out var nextState, args.Threshold) ||
            !_mobThreshold.TryGetThresholdForState(ent, args.MobState.CurrentState, out var currentThreshold, args.Threshold) ||
            !_mobThreshold.TryGetThresholdForState(ent, nextState.Value, out var nextThreshold, args.Threshold))
        {
            _resourceBars.SetFill(ent.Owner, ent.Comp.ResourceBar, 0f);
            return;
        }

        var percentage = (args.Damageable.TotalDamage - currentThreshold.Value) / (nextThreshold.Value - currentThreshold.Value);

        _resourceBars.SetFill(ent.Owner, ent.Comp.ResourceBar, 1f - percentage.Float());
    }
}
