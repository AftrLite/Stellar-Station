// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using System.Numerics;
using Content.Shared.DoAfter;
using Content.Shared.Stunnable;
using Content.Shared.Weather;
using Content.Stellar.Shared.RecyclerChute;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Stellar.Server.RecyclerChute;

public sealed class StellarRecyclerChuteSystem : SharedStellarRecyclerChuteSystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    private readonly ResPath _mapPath = new("Maps/_ST/Other/chute.yml");
    private static readonly EntProtoId<WeatherStatusEffectComponent> ChuteWeather = "StellarWeatherMotionBlur";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarRecyclerChuteStationComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StellarChuteDestinationComponent, ComponentStartup>(OnDestinationStartup);
        SubscribeLocalEvent<StellarChuteDestinationComponent, ComponentShutdown>(OnDestinationShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var chuteQuery = EntityQueryEnumerator<StellarRecyclerChuteComponent>();
        while (chuteQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.AutoActivateTimer is { } timer && Timing.CurTime >= timer && comp.State == ChuteState.Idle)
            {
                comp.AutoActivateTimer = null;
                var container = Container.GetContainer(uid, comp.ContainerId);
                if (container.ContainedEntities.Count == 0)
                    continue;

                comp.State = ChuteState.Charging;
                var doArgs = new DoAfterArgs(EntityManager, uid, comp.ChargeTime, new StellarChargeChuteDoAfterEvent(), uid, uid, uid)
                {
                    BreakOnDamage = true,
                    NeedHand = false,
                };
                Appearance.SetData(uid, ChuteVisuals.Base, ChuteState.Charging);
                DoAfter.TryStartDoAfter(doArgs, out var doAfterId);
                var streamEnt = Audio.PlayPvs(comp.SoundCharge, uid);
                comp.ChargeAudioStream = streamEnt?.Entity;
                comp.DoAfterId = doAfterId;
                Dirty(uid, comp);
            }

            if (comp.CooldownTimer is { } cooldown && Timing.CurTime >= cooldown && comp.State == ChuteState.Cooldown)
            {
                comp.State = ChuteState.Idle;
                comp.CooldownTimer = null;
                PopUp.PopupEntity(Loc.GetString("stellar-chute-popup-cooled"), uid);
                Appearance.SetData(uid, ChuteVisuals.Base, ChuteState.Opening);
                Audio.PlayPvs(comp.SoundOpen, uid);
                Timers.SpawnMethodTimer(TimeSpan.FromSeconds(0.5), () => Appearance.SetData(uid, ChuteVisuals.Base, ChuteState.Idle));
                Dirty(uid, comp);
            }
        }

        var travelQuery = EntityQueryEnumerator<StellarChuteTravellingComponent>();
        while (travelQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.ArrivalTime is { } timer && Timing.CurTime >= timer)
            {
                var dest = Random.Pick(DestinationSet);
                var destCoords = TransformSystem.GetMapCoordinates(dest);

                if (Timing.CurTime >= dest.Comp1.CooldownTimer)
                {
                    Audio.PlayPvs(dest.Comp1.SoundArrive, uid);
                    Spawn(dest.Comp1.ArrivalVfx, destCoords);
                    dest.Comp1.CooldownTimer = Timing.CurTime + dest.Comp1.Cooldown;
                }

                RaiseNetworkEvent(new StellarChuteAnimEvent(GetNetEntity(uid), TimeSpan.Zero, true));
                TransformSystem.SetMapCoordinates(uid, destCoords);
                Physics.ApplyLinearImpulse(uid, new Vector2(Random.NextFloat(-5, +5), Random.NextFloat(-5, +5)) * 30);
                Physics.ApplyAngularImpulse(uid, Random.NextFloat(-12, +12));
                _stun.TryKnockdown(uid, TimeSpan.FromSeconds(1));
                RemComp(uid, comp);
            }
        }
    }

    private void OnMapInit(Entity<StellarRecyclerChuteStationComponent> ent, ref MapInitEvent args)
    {
        DestinationSet.Clear();
        TravelSet.Clear();
        _lookup.GetChildEntities(ent, DestinationSet);

        if (!_mapLoader.TryLoadMap(_mapPath, out var map, out _, new DeserializationOptions() { InitializeMaps = true }))
            return;

        _map.SetPaused(map.Value.Comp.MapId, false);
        _lookup.GetEntitiesOnMap(map.Value.Comp.MapId, TravelSet);
        _weather.TrySetWeather(map.Value.Comp.MapId, ChuteWeather, out _);
    }

    private void OnDestinationStartup(Entity<StellarChuteDestinationComponent> ent, ref ComponentStartup args)
    {
        var xform = Transform(ent);

        DestinationSet.Add((ent.Owner, ent.Comp, xform));
    }

    private void OnDestinationShutdown(Entity<StellarChuteDestinationComponent> ent, ref ComponentShutdown args)
    {
        var xform = Transform(ent);

        if (!DestinationSet.Contains((ent.Owner, ent.Comp, xform)))
            return;

        DestinationSet.Remove((ent.Owner, ent.Comp, xform));
    }
}
