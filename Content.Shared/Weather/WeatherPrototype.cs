using System.Numerics; // Stellar
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Weather;

[Prototype]
public sealed partial class WeatherPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("sprite", required: true)]
    public SpriteSpecifier Sprite = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("color")]
    public Color? Color;

    /// <summary>
    /// Sound to play on the affected areas.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("sound")]
    public SoundSpecifier? Sound;

    /// <summary>
    /// Stellar Station - Parallax scroll speed
    /// </summary>
    [DataField]
    public Vector2 ScrollSpeed = Vector2.Zero;

    /// <summary>
    /// Stellar Station - Opacity
    /// </summary>
    [DataField]
    public float Opacity = 1f;

    /// <summary>
    /// Stellar Station - Universal tile override, applies weather mapwide and ignores tile weather constraints
    /// </summary>
    [DataField]
    public bool Override = false;
}
