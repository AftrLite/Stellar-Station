using Content.Shared._ST.ResourceBars;
using Content.Shared._ST.ResourceBars.Components;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._ST.ResourceBars;

/// <summary>
/// Client version of Resource Bar System.
/// </summary>
public sealed class ResourceBarsSystem : SharedResourceBarsSystem
{
    public event Action<Entity<ResourceBarsComponent>>? OnPlayerBarsStartup;
    public event Action? OnPlayerBarsShutdown;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceBarsComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<ResourceBarsComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);
    }

    #region Gui
    private void HandlePlayerAttached(Entity<ResourceBarsComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        OnPlayerBarsStartup?.Invoke(ent);
    }

    private void HandlePlayerDetached(Entity<ResourceBarsComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        OnPlayerBarsShutdown?.Invoke();
    }

    #endregion
}
