using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Robust.Shared.Reflection;

namespace Content.Client._ST.Controls;

public sealed class DeferredTypeContainer : PanelContainer
{
    private string _control = string.Empty;
    public string Control
    {
        get => _control;
        set
        {
            if (IoCManager.Resolve<IReflectionManager>().GetType(value) is not { } type)
                throw new InvalidOperationException();

            _control = value;
            var control = (Control)IoCManager.Resolve<IDynamicTypeFactory>().CreateInstance(type);

            RemoveAllChildren();
            AddChild(control);
        }
    }
}
