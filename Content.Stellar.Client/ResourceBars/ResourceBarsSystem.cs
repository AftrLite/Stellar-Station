// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.ResourceBars;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Client.ResourceBars;

public sealed class ResourceBarsSystem : SharedResourceBarsSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action? ClearResourceBars;
    public event Action<IReadOnlyDictionary<ProtoId<ResourceBarPrototype>, ResourceBarState>>? UpdateResourceBars;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceBarsComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<ResourceBarsComponent, LocalPlayerAttachedEvent>(OnLocalPlayerAttached);
        SubscribeLocalEvent<ResourceBarsComponent, LocalPlayerDetachedEvent>(OnLocalPlayerDetached);
        SubscribeLocalEvent<ResourceBarsComponent, AfterAutoHandleStateEvent>(OnAutoHandleState);
    }

    protected override void AfterUpdateBars(Entity<ResourceBarsComponent> ent)
    {
        if (_playerManager.LocalEntity != ent.Owner)
            return;

        UpdateResourceBars?.Invoke(ent.Comp.Bars);
    }

    private void OnAutoHandleState(Entity<ResourceBarsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_playerManager.LocalEntity != ent.Owner)
            return;

        UpdateResourceBars?.Invoke(ent.Comp.Bars);
    }

    private void OnLocalPlayerAttached(Entity<ResourceBarsComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        if (_playerManager.LocalEntity != ent.Owner)
            return;

        UpdateResourceBars?.Invoke(ent.Comp.Bars);
    }

    private void OnLocalPlayerDetached(Entity<ResourceBarsComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        if (_playerManager.LocalEntity != ent.Owner)
            return;

        ClearResourceBars?.Invoke();
    }

    private void OnComponentShutdown(Entity<ResourceBarsComponent> ent, ref ComponentShutdown args)
    {
        if (_playerManager.LocalEntity != ent.Owner)
            return;

        ClearResourceBars?.Invoke();
    }
}
