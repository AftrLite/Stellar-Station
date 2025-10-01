using Robust.Shared.Utility;

namespace Content.Shared.Atmos.Components;

[RegisterComponent]
public sealed partial class PipeAppearanceComponent : Component
{
    [DataField]
    public SpriteSpecifier.Rsi[] Sprite = [new(new("/Textures/_ST/Tileset/Atmos/piping.rsi"), "pipeConnector"), // Begin Stellar
        new(new("/Textures/_ST/Tileset/Atmos/piping_alt1.rsi"), "pipeConnector"),
        new(new("/Textures/_ST/Tileset/Atmos/piping_alt2.rsi"), "pipeConnector")]; // End Stellar
}
