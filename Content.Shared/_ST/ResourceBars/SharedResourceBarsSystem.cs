using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._ST.ResourceBars.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._ST.ResourceBars;

public abstract class SharedResourceBarsSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private FrozenDictionary<ProtoId<ResourceBarPrototype>, ResourceBarPrototype> _verifyResourceProto = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceBarsComponent, ComponentStartup>(HandleComponentStartup);
        SubscribeLocalEvent<ResourceBarsComponent, ComponentShutdown>(HandleComponentShutdown);
        SubscribeLocalEvent<ResourceBarsComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(HandlePrototypesReloaded);
        LoadPrototypes();
    }

    public void AddResourceBar(EntityUid ent, ProtoId<ResourceBarPrototype> resourceProto)
    {
        if (!TryGetResource(resourceProto, out var proto) || !HasResourceBar(ent, resourceProto))
            return;

        EnsureComp<ResourceBarsComponent>(ent, out var resBarComp);
        resBarComp.ActiveResourceBars.Add(proto);

        if (proto.AssociatedComponents != null)
        {
            foreach (var reg in proto.AssociatedComponents.Values)
            {
                var compType = reg.Component.GetType();
                if (HasComp(ent, compType))
                    continue;
                AddComp(ent, _componentFactory.GetComponent(compType));
            }
        }
        Dirty(ent, resBarComp);
    }

    public bool TryGetResource(ProtoId<ResourceBarPrototype> resourceProto, [NotNullWhen(true)] out ResourceBarPrototype? proto)
    {
        return _verifyResourceProto.TryGetValue(resourceProto, out proto);
    }

    public bool HasResourceBar(EntityUid ent, ProtoId<ResourceBarPrototype> resourceProto)
    {
        if (!TryGetResource(resourceProto, out var proto) || !TryComp(ent, out ResourceBarsComponent? resBarComp))
            return false;
        if (resBarComp.ActiveResourceBars.Contains(proto))
            return true;
        else return false;
    }

    #region BORING STUFF

    private void OnPlayerAttached(Entity<ResourceBarsComponent> ent, ref PlayerAttachedEvent args)
    {
        Dirty(ent);
    }

    protected virtual void HandleComponentShutdown(Entity<ResourceBarsComponent> ent, ref ComponentShutdown args)
    {
        RaiseLocalEvent(ent, new ResourceBarSyncEvent(ent), true);
    }

    private void HandleComponentStartup(Entity<ResourceBarsComponent> ent, ref ComponentStartup args)
    {
        RaiseLocalEvent(ent, new ResourceBarSyncEvent(ent), true);
    }

    private void HandlePrototypesReloaded(PrototypesReloadedEventArgs obj)
    {
        if (obj.WasModified<ResourceBarPrototype>())
            LoadPrototypes();
    }

    protected virtual void LoadPrototypes()
    {
        var dict = new Dictionary<ProtoId<ResourceBarPrototype>, ResourceBarPrototype>();
        foreach (var resourceBar in _prototype.EnumeratePrototypes<ResourceBarPrototype>())
        {
            if (!dict.TryAdd(resourceBar.ID, resourceBar))
            {
                Log.Error("Found resource bar with duplicate ID {0} - all resource bars must have" +
                          " a unique ID, this one will be skipped", resourceBar.ID);
            }
        }

        _verifyResourceProto = dict.ToFrozenDictionary();
    }

    #endregion

}
