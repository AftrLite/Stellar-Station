using Content.Client._ST.ResourceBars.Controls;
using Content.Client._ST.ResourceBars.Widgets;
using Content.Client.Gameplay;
using Content.Shared._ST.ResourceBars.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Utility;

namespace Content.Client._ST.ResourceBars;

public sealed class ResourceBarsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<ResourceBarsSystem>
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [UISystemDependency] private readonly ResourceBarsSystem _resourceBars = default!;
    private ResourceBarsComponent? _playerResBarComp;
    private ResourceBarsGui? BarGui => UIManager.GetActiveUIWidgetOrNull<ResourceBarsGui>();

    public void OnSystemLoaded(ResourceBarsSystem system)
    {
        _resourceBars.OnPlayerBarsStartup += LoadPlayerBars;
        _resourceBars.OnPlayerBarsShutdown += UnloadPlayerBars;
    }

    public void OnSystemUnloaded(ResourceBarsSystem system)
    {
        _resourceBars.OnPlayerBarsStartup -= LoadPlayerBars;
        _resourceBars.OnPlayerBarsShutdown -= UnloadPlayerBars;
    }
    public void OnStateEntered(GameplayState state)
    {
        if (BarGui != null)
            BarGui.Visible = true;
    }

    public void OnStateExited(GameplayState state)
    {
        if (BarGui != null)
            BarGui.Visible = false;
    }

    public ResourceBarsComponent? GetResouceBarData()
    {
        if (_playerResBarComp is not null)
            return _playerResBarComp;
        else return null;
    }
    private void LoadPlayerBars(Entity<ResourceBarsComponent> comp)
    {
        DebugTools.Assert(_playerResBarComp == null);
        if (BarGui != null)
            BarGui.Visible = true;

        _playerResBarComp = comp;
    }
    private void UnloadPlayerBars()
    {
        if (BarGui != null)
            BarGui.Visible = false;

        _playerResBarComp = null;
    }

    // public void RegisterResourceBarContainer(ResourceBarsContainer resourceBar)
    // {
    //     _resourceBars = resourceBars;
    // }

}
