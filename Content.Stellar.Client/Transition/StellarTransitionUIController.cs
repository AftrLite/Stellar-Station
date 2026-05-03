using System.Numerics;
using Content.Client.Resources;
using JetBrains.Annotations;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Console;
using Robust.Shared.Timing;

namespace Content.Stellar.Client.Transition;

/// <summary>
///     Handles the fade animation when lobby->game or gameend->lobby transitions
///     Creates controls on init and attaches them to the root control, sorry
/// </summary>

[UsedImplicitly]
public sealed class StellarTransitionUIController : UIController
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IConsoleHost _conHost = default!;
    [Dependency] private readonly IResourceCache _resCache = default!;

    public TransitionState TransitionState { get; private set; } = TransitionState.PlayScreen;

    public bool IsOpen => TransitionState == TransitionState.PlayScreen;
    public bool IsClosed => TransitionState == TransitionState.BlackScreen;

    private LayoutContainer _transitionRoot = default!;
    private TextureRect _fader = default!;

    private static readonly TimeSpan DefaultAnimationTime = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan ClosedPanicOpenTime = TimeSpan.FromSeconds(10);
    private float _currentTargetTime;
    private float _accumulatedTime;
    private float _timeSpentClosed; // measured so we can panic-unfade if we're a black screen too long for some reason

    public override void Initialize()
    {
        base.Initialize();

        _conHost.RegisterCommand("toggletransition", "Toggles the screen transition animation", "toggletransition", (_, _, _) => StartTransition(TransitionState < TransitionState.FadeIn));

        CreateTransitionControls();
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (TransitionState is TransitionState.BlackScreen)
        {
            _timeSpentClosed += args.DeltaSeconds;
            if (_timeSpentClosed > ClosedPanicOpenTime.TotalSeconds)
            {
                Log.Info("black-screen panic time exceeded: transitioning screen Gameplay.");
                StartTransition(true, TimeSpan.FromSeconds(0.5));
                _timeSpentClosed = 0f;
                return;
            }
        }
        else
        {
            _timeSpentClosed = 0f;
        }

        if (TransitionState is not (TransitionState.FadeOut or TransitionState.FadeIn))
            return;

        _accumulatedTime += args.DeltaSeconds;

        var t = Easings.InOutQuad(Math.Clamp(_accumulatedTime / _currentTargetTime, 0f, 1f));
        var fadeBase = TransitionState is TransitionState.FadeIn ? t : 1-t;
        var fade = MathHelper.Lerp(1, 0, fadeBase);

        _fader.Modulate = Color.Black.WithAlpha(fade);

        if (_accumulatedTime < _currentTargetTime)
            return;

        _accumulatedTime = 0f;

        TransitionState = TransitionState switch
        {
            TransitionState.FadeOut => TransitionState.BlackScreen,
            TransitionState.FadeIn => TransitionState.PlayScreen,
            _ => TransitionState,
        };

        if (TransitionState == TransitionState.PlayScreen)
        {
            _fader.Visible = false;
        }
    }

    /// <summary>
    ///     Creates the root for the fade animation and attaches them to the UI root
    /// </summary>
    private void CreateTransitionControls()
    {
        _transitionRoot = new LayoutContainer { Name = "TransitionRoot" };
        _ui.RootControl.AddChild(_transitionRoot);

        _fader = new TextureRect
        {
            Stretch = TextureRect.StretchMode.Scale,
            Texture =
                _resCache.GetTexture("/Textures/_ST/Interface/Misc/transition.png"),
            Visible = false,
        };
        _transitionRoot.AddChild(_fader);
    }

    /// <summary>
    /// starts a transition animation, either FadeIn or FadeOut
    /// </summary>
    /// <param name="toOpen">whether to FadeIn or FadeOut</param>
    /// <param name="animationTimeOverride">the amount of time the animation should take</param>
    public void StartTransition(bool toOpen, TimeSpan? animationTimeOverride = null)
    {
        if ((toOpen && TransitionState > TransitionState.FadeOut) ||
            (!toOpen && TransitionState < TransitionState.FadeIn))
            return;

        TransitionState = toOpen ? TransitionState.FadeIn : TransitionState.FadeOut;
        _currentTargetTime = animationTimeOverride is not null
            ? (float)animationTimeOverride.Value.TotalSeconds
            : (float)DefaultAnimationTime.TotalSeconds;

        Log.Info($"Playing transition: {TransitionState} for {Math.Round(_currentTargetTime, 2)} seconds");

        _fader.SetWidth = _transitionRoot.Width;
        _fader.SetHeight = _transitionRoot.Height;
        _fader.Visible = true;

        if (!toOpen)
            _fader.Modulate = Color.Black.WithAlpha(1);
        else
            _fader.Modulate = Color.Black.WithAlpha(1);
    }
}

public enum TransitionState : byte
{
    BlackScreen = 0,
    FadeOut = 1,
    FadeIn = 2,
    PlayScreen = 3,
}
