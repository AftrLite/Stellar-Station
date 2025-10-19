using System.Numerics;
using Content.Client.UserInterface;
using Content.Shared._ST.ResourceBars;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface;

namespace Content.Client._ST.ResourceBars;

public sealed partial class ResourceBarControl : Control
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private const float ResourceScale = 2;
    private readonly SpriteSystem _sprite;
    private readonly ResourceUIPosition _position;
    private ResourceBarState _state;

    public readonly TextureRect BarIcon;
    public readonly TextureRect BarForegroundRect;
    public readonly TextureRect BarBackgroundRect;
    public readonly TextureRect BarFrameRect;

    private string _foregroundTexturePath = default!;
    public string ForegroundTexturePath
    {
        get => _foregroundTexturePath;
        set
        {
            _foregroundTexturePath = value;
            BarForegroundRect.Texture = Theme.ResolveTexture(value);

            var expectedSize = BarForegroundRect.TextureSizeTarget;
            expectedSize.X *= _state.Fill;
            BarForegroundRect.SetSize = expectedSize;
        }
    }

    private string _backgroundTexturePath = default!;
    public string BackgroundTexturePath
    {
        get => _backgroundTexturePath;
        set
        {
            _backgroundTexturePath = value;
            BarBackgroundRect.Texture = Theme.ResolveTexture(value);
            BarBackgroundRect.SetSize = BarBackgroundRect.TextureSizeTarget;
        }
    }

    private string _frameTexturePath = default!;
    public string FrameTexturePath
    {
        get => _frameTexturePath;
        set
        {
            _frameTexturePath = value;
            BarFrameRect.Texture = Theme.ResolveTexture(value);
            BarFrameRect.SetSize = BarFrameRect.TextureSizeTarget;
        }
    }

    public ResourceBarControl(ResourceBarPrototype proto, ResourceBarState state)
    {
        IoCManager.InjectDependencies(this);
        _sprite = _entityManager.System<SpriteSystem>();
        _position = proto.Location;
        _state = state;

        AddChild(BarFrameRect = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            MouseFilter = MouseFilterMode.Ignore,
            Name = "Frame",
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top
        });
        AddChild(BarBackgroundRect = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            MouseFilter = MouseFilterMode.Ignore,
            Name = "Background",
            Modulate = proto.Color,
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top
        });
        AddChild(BarForegroundRect = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            MouseFilter = MouseFilterMode.Ignore,
            Name = "Foreground",
            Modulate = proto.Color,
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top,
            RectClipContent = true
        });
        AddChild(BarIcon = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            Texture = _sprite.Frame0(proto.Icon),
            MouseFilter = MouseFilterMode.Ignore,
            Name = "Icon",
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top
        });
        BarIcon.SetSize = BarIcon.TextureSizeTarget;

        FrameTexturePath = proto.Location switch
        {
            ResourceUIPosition.Left => "Bars/bar_left",
            ResourceUIPosition.Middle => "Bars/bar_middle",
            ResourceUIPosition.Right => "Bars/bar_right",
            _ => throw new ArgumentOutOfRangeException(nameof(proto), proto, null)
        };

        BackgroundTexturePath = "Bars/bar_background";
        ForegroundTexturePath = "Bars/bar_foreground";

        UpdateMargins();
    }

    private void UpdateMargins()
    {
        Thickness barMargin;
        Thickness iconMargin;

        switch (_position)
        {
            case ResourceUIPosition.Left:
                barMargin = MarginFromThemeColor("_resource_bar_left_bar_margins");
                iconMargin = MarginFromThemeColor("_resource_bar_left_icon_margins");
                break;
            case ResourceUIPosition.Middle:
                barMargin = MarginFromThemeColor("_resource_bar_middle_bar_margins");
                iconMargin = MarginFromThemeColor("_resource_bar_middle_icon_margins");
                break;
            case ResourceUIPosition.Right:
                barMargin = MarginFromThemeColor("_resource_bar_right_bar_margins");
                iconMargin = MarginFromThemeColor("_resource_bar_right_icon_margins");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_position), _position, null);
        }

        BarBackgroundRect.Margin = barMargin.Scale(ResourceScale);
        BarForegroundRect.Margin = barMargin.Scale(ResourceScale);
        BarIcon.Margin = iconMargin.Scale(ResourceScale);
    }

    private Thickness MarginFromThemeColor(string itemName)
    {
        var color = Theme.ResolveColorOrSpecified(itemName);
        return new Thickness(color.RByte, color.GByte, color.BByte, color.AByte);
    }

    protected override void OnThemeUpdated()
    {
        base.OnThemeUpdated();

        BarFrameRect.Texture = Theme.ResolveTexture(_frameTexturePath);
        BarFrameRect.SetSize = BarFrameRect.TextureSizeTarget;
        BarBackgroundRect.Texture = Theme.ResolveTexture(_backgroundTexturePath);
        BarBackgroundRect.SetSize = BarBackgroundRect.TextureSizeTarget;
        BarForegroundRect.Texture = Theme.ResolveTexture(_foregroundTexturePath);
        BarForegroundRect.SetSize = BarForegroundRect.TextureSizeTarget;
        UpdateMargins();
    }

    public void Update(ResourceBarState state)
    {
        var expectedSize = BarForegroundRect.TextureSizeTarget;
        expectedSize.X *= state.Fill;
        BarForegroundRect.SetSize = expectedSize;
    }
}
