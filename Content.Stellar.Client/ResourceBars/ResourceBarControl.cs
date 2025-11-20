// SPDX-FileCopyrightText: 2025 AftrLite
// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Client.Actions.UI;
using Content.Client.UserInterface;
using Content.Stellar.Shared.ResourceBars;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Stellar.Client.ResourceBars;

public sealed partial class ResourceBarControl : Control
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private const float ResourceScale = 2;
    private readonly SpriteSystem _sprite;
    private readonly ResourceUIPosition _position;
    private readonly ResourceUIStyle _style;
    private ResourceBarState _state;

    public readonly TextureRect BarIcon;
    public readonly TextureRect BarForegroundRect;
    public readonly TextureRect BarBackgroundRect;
    public readonly TextureRect BarFrameRect;
    public readonly ProtoId<ResourceBarPrototype> BarPrototype;

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
        BarPrototype = proto.ID;
        _sprite = _entityManager.System<SpriteSystem>();
        _position = proto.Location;
        _style = proto.Style;
        _state = state;

        var msg = FormattedMessage.FromMarkupOrThrow(Loc.GetString(proto.Title));
        var desc = FormattedMessage.FromMarkupOrThrow(Loc.GetString(proto.Description));
        var tooltip = new ActionAlertTooltip(msg, desc);
        TooltipSupplier = _ => tooltip;

        AddChild(BarFrameRect = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            Name = "Frame",
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top,
            TooltipSupplier = _ => tooltip
        });
        AddChild(BarBackgroundRect = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            Name = "Background",
            Modulate = proto.Color,
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top,
            TooltipSupplier = _ => tooltip
        });
        AddChild(BarForegroundRect = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            Name = "Foreground",
            Modulate = proto.Color,
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top,
            RectClipContent = true,
            TooltipSupplier = _ => tooltip
        });
        AddChild(BarIcon = new TextureRect
        {
            TextureScale = new Vector2(ResourceScale, ResourceScale),
            Texture = _sprite.Frame0(proto.Icon),
            Name = "Icon",
            HorizontalAlignment = Control.HAlignment.Left,
            VerticalAlignment = Control.VAlignment.Top,
            TooltipSupplier = _ => tooltip
        });
        BarIcon.SetSize = BarIcon.TextureSizeTarget;

        switch (proto.Style)
        {
            case ResourceUIStyle.Thin:
                FrameTexturePath = proto.Location switch
                {
                    ResourceUIPosition.Left => "Bars/bar_thin_left",
                    ResourceUIPosition.Middle => "Bars/bar_thin_middle",
                    ResourceUIPosition.Right => "Bars/bar_thin_right",
                    _ => throw new ArgumentOutOfRangeException(nameof(proto.Location), proto.Location, null)
                };
                BackgroundTexturePath = "Bars/bar_thin_background";
                ForegroundTexturePath = "Bars/bar_thin_foreground";
                break;
            case ResourceUIStyle.Default:
                FrameTexturePath = proto.Location switch
                {
                    ResourceUIPosition.Left => "Bars/bar_left",
                    ResourceUIPosition.Middle => "Bars/bar_middle",
                    ResourceUIPosition.Right => "Bars/bar_right",
                    _ => throw new ArgumentOutOfRangeException(nameof(proto.Location), proto.Location, null)
                };
                BackgroundTexturePath = "Bars/bar_background";
                ForegroundTexturePath = "Bars/bar_foreground";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(proto.Style), proto.Style, null);
        }

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
                if (_style == ResourceUIStyle.Thin)
                {
                    barMargin = MarginFromThemeColor("_resource_bar_middle_bar_thin_margins");
                    iconMargin = MarginFromThemeColor("_resource_bar_middle_icon_thin_margins");
                }
                else
                {
                    barMargin = MarginFromThemeColor("_resource_bar_middle_bar_margins");
                    iconMargin = MarginFromThemeColor("_resource_bar_middle_icon_margins");
                }
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
