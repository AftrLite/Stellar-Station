using System.Numerics;
using Content.Client.UserInterface;
using Content.Shared._ST.ResourceBars;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._ST.ResourceBars.Controls
{
    [Virtual]
    public abstract class ResouceBarControl : Control
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        private readonly SpriteSystem _sprite;
        public static Vector2 DefaultBarSize = new Vector2(158, 14);
        public Vector2 BarFrameSize;
        public TextureRect BarIcon { get; }
        public TextureRect BarForegroundRect { get; }
        public TextureRect BarBackgroundRect { get; }
        public TextureRect BarFrameRect { get; }
        public readonly ResourceBarPrototype BarProto;
        private string? _foregroundTexturePath;
        public string? ForegroundTexturePath
        {
            get => _foregroundTexturePath;
            set
            {
                _foregroundTexturePath = value;
                BarForegroundRect.Texture = Theme.ResolveTextureOrNull(_foregroundTexturePath)?.Texture;
            }
        }

        private string? _backgroundTexturePath;
        public string? BackgroundTexturePath
        {
            get => _backgroundTexturePath;
            set
            {
                _foregroundTexturePath = value;
                BarBackgroundRect.Texture = Theme.ResolveTextureOrNull(_backgroundTexturePath)?.Texture;
            }
        }

        private string? _frameTexturePath;
        public string? FrameTexturePath
        {
            get => _frameTexturePath;
            set
            {
                _frameTexturePath = value;
                BarFrameRect.Texture = Theme.ResolveTextureOrNull(_frameTexturePath)?.Texture;
            }
        }

        public ResouceBarControl(ResourceBarPrototype proto, ResourceUIPosition location)
        {
            IoCManager.InjectDependencies(this);
            _sprite = _entityManager.System<SpriteSystem>();
            Name = "ResourceBar_Null";
            BarProto = proto;

            switch (location)
            {
                case ResourceUIPosition.Left:
                    FrameTexturePath = "/Bars/bar_left";
                    BarFrameSize = new Vector2(196, 30);
                    break;
                case ResourceUIPosition.Middle:
                    FrameTexturePath = "/Bars/bar_middle";
                    BarFrameSize = new Vector2(170, 30);
                    break;
                case ResourceUIPosition.Right:
                    FrameTexturePath = "/Bars/bar_right";
                    BarFrameSize = new Vector2(196, 30);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(location), location, null);
            }

            AddChild(BarFrameRect = new TextureRect
            {
                TextureScale = new Vector2(2, 2),
                SetSize = BarFrameSize,
                MouseFilter = MouseFilterMode.Ignore
            });
            AddChild(BarBackgroundRect = new TextureRect
            {
                TextureScale = new Vector2(2, 2),
                SetSize = DefaultBarSize,
                MouseFilter = MouseFilterMode.Ignore
            });
            AddChild(BarForegroundRect = new TextureRect
            {
                TextureScale = new Vector2(2, 2),
                SetSize = DefaultBarSize,
                MouseFilter = MouseFilterMode.Ignore
            });
            AddChild(BarIcon = new TextureRect
            {
                TextureScale = new Vector2(2, 2),
                Texture = _sprite.Frame0(BarProto.Icon),
                MouseFilter = MouseFilterMode.Ignore
            });

            BackgroundTexturePath = "/Bars/bar_background";
            ForegroundTexturePath = "/Bars/bar_foreground";
        }

        protected override void OnThemeUpdated()
        {
            base.OnThemeUpdated();
            BarFrameRect.Texture = Theme.ResolveTextureOrNull(_frameTexturePath)?.Texture;
            BarBackgroundRect.Texture = Theme.ResolveTextureOrNull(_backgroundTexturePath)?.Texture;
            BarForegroundRect.Texture = Theme.ResolveTextureOrNull(_foregroundTexturePath)?.Texture;
        }
    }
}
