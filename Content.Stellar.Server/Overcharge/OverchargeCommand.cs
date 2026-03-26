// SPDX-FileCopyrightText: 2026 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Stellar.Shared.Overcharge;
using Content.Stellar.Shared.Overcharge.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;

namespace Content.Stellar.Server.Overcharge;

[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class OverchargeCommand : ToolshedCommand
{
    private StellarOverchargeSystem? _overcharge;

    [CommandImplementation("set")]
    public EntityUid Set([PipedArgument] EntityUid station, ProtoId<StellarOverchargePrototype> prototype, OverchargeState state)
    {
        _overcharge ??= GetSys<StellarOverchargeSystem>();
        _overcharge.ToggleOvercharge(station, prototype, state);
        return station;
    }

    [CommandImplementation("clear")]
    public EntityUid Clear([PipedArgument] EntityUid station)
    {
        _overcharge ??= GetSys<StellarOverchargeSystem>();
        _overcharge.ToggleOvercharge(station, null, OverchargeState.Disabled);
        return station;
    }
}
