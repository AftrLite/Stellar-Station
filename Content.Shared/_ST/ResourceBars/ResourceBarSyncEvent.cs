namespace Content.Shared._ST.ResourceBars;

/// <summary>
///     Raised when the ResouceBarsSystem might need resouce bar sources to recalculate stuff.
/// </summary>
public sealed class ResourceBarSyncEvent : EntityEventArgs
{
    public EntityUid Entity { get; }

    public ResourceBarSyncEvent(EntityUid ent)
    {
        Entity = ent;
    }
}
