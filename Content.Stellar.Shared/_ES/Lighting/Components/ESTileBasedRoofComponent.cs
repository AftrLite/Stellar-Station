// SPDX-FileCopyrightText: 2025 EmoGarbage404
//
// SPDX-License-Identifier: MIT

using Content.Shared.Maps;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ES.Lighting.Components;

/// <summary>
/// Applies roofs to a grid based on the tiles on the floor.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ESTileBasedRoofComponent : Component
{
    /// <summary>
    /// Which tiles count as unroofed
    /// </summary>
    // TODO: ideally this is on the prototype but for my life i cannot be fucked with it.
    [DataField]
    public HashSet<ProtoId<ContentTileDefinition>> UnRoofedTiles = new()
    {
        "Lattice", // Space structures
        "FloorGlass", // See-through tiles
        "FloorRGlass",
        "FloorAsteroidSand", // natural non-station tiles
        "PlatingAsteroid",
        "FloorCosmicVoid",
        "StellarFloorNothing",
        "StellarFloorGlassBase",
        "StellarFloorGlassDouble1",
        "StellarFloorGlassDouble2",
        "StellarFloorGlassMono",
        "StellarFloorGlassMulti1",
        "StellarFloorGlassMulti2",
        "StellarFloorGlassFramedDouble1",
        "StellarFloorGlassFramedDouble2",
        "StellarFloorGlassFramedMono",
        "StellarFloorGlassPlasmaBase",
        "StellarFloorGlassPlasmaDouble1",
        "StellarFloorGlassPlasmaDouble2",
        "StellarFloorGlassPlasmaMono",
        "StellarFloorGlassPlasmaMulti1",
        "StellarFloorGlassPlasmaMulti2",
        "StellarFloorGlassPlasmaFramedDouble1",
        "StellarFloorGlassPlasmaFramedDouble2",
        "StellarFloorGlassPlasmaFramedMono",
        "StellarFloorGlassUraniumBase",
        "StellarFloorGlassUraniumDouble1",
        "StellarFloorGlassUraniumDouble2",
        "StellarFloorGlassUraniumMono",
        "StellarFloorGlassUraniumMulti1",
        "StellarFloorGlassUraniumMulti2",
        "StellarFloorGlassUraniumFramedDouble1",
        "StellarFloorGlassUraniumFramedDouble2",
        "StellarFloorGlassUraniumFramedMono",
    };
}
