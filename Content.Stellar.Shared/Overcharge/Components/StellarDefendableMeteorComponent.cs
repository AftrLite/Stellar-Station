// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Stellar.Shared.Overcharge.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StellarDefendableMeteorComponent : Component
{
    /// <summary>
    /// Damage specifier that is multiplied against the calculated damage amount to determine what damage is applied to the colliding entity.
    /// </summary>
    /// <remarks>
    /// The values of this should add up to 1 or else the damage will be scaled.
    /// </remarks>
    [DataField]
    public DamageSpecifier DamageTypes = new();

    /// <summary>
    /// A list of entities that this meteor has collided with. used to ensure no double collisions occur.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> HitList = new();

    /// <summary>
    /// The sound effect for when a meteor is mitigated.
    /// </summary>
    [DataField]
    public SoundSpecifier MitigationSfx = new SoundPathSpecifier("/Audio/_ST/Overcharge/shield-mitigate.ogg");
}

/// <summary>
/// Raised on a station when a defendable meteor collides with an entity on it
/// </summary>
/// <param name="Defended">Whether the meteor has been defended against</param>
[ByRefEvent]
public record struct StellarDefendableMeteorCollidedEvent(bool Defended);
