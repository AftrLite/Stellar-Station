// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Server.PowerCell;
using Content.Shared.Power;
using Content.Shared.PowerCell.Components;
using Content.Stellar.Shared.Mobs;
using Content.Stellar.Shared.ResourceBars;

namespace Content.Stellar.Server.Mobs;

public sealed class BatteryResourceBarSystem : EntitySystem
{
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedResourceBarsSystem _resourceBars = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryResourceBarComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BatteryResourceBarComponent, PowerCellChangedEvent>(OnPowerCellChanged);
        SubscribeLocalEvent<BatteryResourceBarComponent, ChangeChargeEvent>(OnChargeChanged);
        SubscribeLocalEvent<BatteryResourceBarComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMapInit(Entity<BatteryResourceBarComponent> ent, ref MapInitEvent args)
    {
        UpdateResourceBars(ent);
    }

    private void OnPowerCellChanged(Entity<BatteryResourceBarComponent> ent, ref PowerCellChangedEvent args)
    {
        UpdateResourceBars(ent);
    }

    private void OnChargeChanged(Entity<BatteryResourceBarComponent> ent, ref ChangeChargeEvent args)
    {
        UpdateResourceBars(ent);
    }

    private void OnShutdown(Entity<BatteryResourceBarComponent> ent, ref ComponentShutdown args)
    {
        _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBar);
    }

    private void UpdateResourceBars(Entity<BatteryResourceBarComponent> ent)
    {
        if (!_powerCell.TryGetBatteryFromSlot(ent, out var battery))
        {
            _resourceBars.ClearResourceBar(ent.Owner, ent.Comp.ResourceBar);
            return;
        }

        _resourceBars.ShowResourceBar(ent.Owner, ent.Comp.ResourceBar, battery.CurrentCharge / battery.MaxCharge);
    }
}
