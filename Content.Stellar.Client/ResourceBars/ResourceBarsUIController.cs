using Content.Client.Gameplay;
using Content.Stellar.Shared.ResourceBars;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.ResourceBars;

public sealed class ResourceBarsUIController : UIController, IOnSystemChanged<ResourceBarsSystem>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    [UISystemDependency] private readonly ResourceBarsSystem _resourceBars = default!;

    private ResourceBarsUI? BarUI => UIManager.GetActiveUIWidgetOrNull<ResourceBarsUI>();

    public void OnSystemLoaded(ResourceBarsSystem system)
    {
        _resourceBars.ClearResourceBars += ClearResourceBars;
        _resourceBars.UpdateResourceBars += UpdateResourceBars;
    }

    public void OnSystemUnloaded(ResourceBarsSystem system)
    {
        _resourceBars.ClearResourceBars -= ClearResourceBars;
        _resourceBars.UpdateResourceBars -= UpdateResourceBars;
    }

    private void ClearResourceBars()
    {
        BarUI?.Clear();
    }

    private void UpdateResourceBars(IReadOnlyDictionary<ProtoId<ResourceBarPrototype>, ResourceBarState> bars)
    {
        BarUI?.Update(_prototype, bars);
    }
}
