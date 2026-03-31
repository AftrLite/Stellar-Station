// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Stellar.Shared.Explosion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedStellarExplosionsSystem))]
public sealed partial class StellarExplosiveComponent : Component
{
    /// <summary>
    /// The explosion entity spawned by this explosive, which handles the visual effects.
    /// </summary>
    [DataField, AutoNetworkedField] public EntProtoId? Explosion = "StellarExplosionDefault";

    /// <summary>
    /// Explosion range used for Damage and Knockback. Does not affect the explosion's visuals.
    /// </summary>
    [DataField, AutoNetworkedField] public float Range = 5;

    [DataField, AutoNetworkedField] public float ShakeIntensity = 2;

    /// <summary>
    /// Does this explosion inflict knockback?
    /// </summary>
    [DataField] public bool Knockback;

    /// <summary>
    /// Does this explosion inflict knockdown?
    /// </summary>
    [DataField] public bool Knockdown;

    [DataField] public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Does this explosion inflict stun?
    /// </summary>
    [DataField] public bool Stun;

    [DataField] public TimeSpan StunDuration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The speed of the knockback inflicted by the explosion.
    /// </summary>
    [DataField, AutoNetworkedField] public float KnockbackSpeed = 8;

    [DataField, AutoNetworkedField] public bool IgnoreObstruction;

    /// <summary>
    /// What kind of entities should the knockback apply to?
    /// </summary>
    [DataField, AutoNetworkedField] public EntityWhitelist? Whitelist;

    /// <summary>
    /// If set, allows the entity to trigger Stellar explosions off of the TriggerSystem.
    /// </summary>
    [DataField] public string? TriggerKey;

    /// <summary>
    /// Sound played for entities within range of the explosion. Sound range is explosion range * 2.
    /// </summary>
    [DataField] public SoundSpecifier? Sound = new SoundCollectionSpecifier("ExplosionSmall");

    /// <summary>
    /// Sound played for entities "far away" from the explosion. "Far" range is explosion range * 5.
    /// </summary>
    [DataField] public SoundSpecifier? SoundFar = new SoundCollectionSpecifier("ExplosionSmallFar");

    /// <summary>
    /// Shrapnel emitted by this explosion.
    /// </summary>
    [DataField, AutoNetworkedField] public List<EntProtoId>? ShrapnelEffects;

    [DataField, AutoNetworkedField] public int MinShrapnel = 0;

    [DataField, AutoNetworkedField] public int MaxShrapnel = 0;

    [DataField, AutoNetworkedField] public float ShrapnelSpeed = 30;
}
