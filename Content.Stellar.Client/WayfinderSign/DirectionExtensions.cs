// SPDX-FileCopyrightText: 2025 Janet Blackquill
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Client.GameObjects;

namespace Content.Stellar.Client.WayfinderSign;

public static class DirectionExtensions
{
    public static SpriteComponent.DirectionOffset ToOffset(this Direction direction)
    {
        return direction switch {
            Direction.North => SpriteComponent.DirectionOffset.Flip,
            Direction.East => SpriteComponent.DirectionOffset.CounterClockwise,
            Direction.South => SpriteComponent.DirectionOffset.None,
            Direction.West => SpriteComponent.DirectionOffset.Clockwise,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "invalid direction")
        };
    }
}
