using Architect.Attributes.Broadcasters;

namespace Architect.Content.Groups;

public class BroadcasterGroup
{
    internal static readonly BroadcasterGroup EmptyGroup = new();
    
    public static readonly BroadcasterGroup Enemies = new(
        EventBroadcasterType.OnDeath,
        EventBroadcasterType.LoadedDead
    );
    
    public static readonly BroadcasterGroup Levers = new(
        EventBroadcasterType.OnPull,
        EventBroadcasterType.LoadedPulled
    );
    
    public static readonly BroadcasterGroup PersistentLevers = new(
        EventBroadcasterType.OnPull,
        EventBroadcasterType.LoadedPulled
    );
    
    public static readonly BroadcasterGroup TriggerZones = new(
        EventBroadcasterType.ZoneEnter,
        EventBroadcasterType.ZoneExit
    );
    
    public static readonly BroadcasterGroup Bindings = new(
        EventBroadcasterType.OnBind,
        EventBroadcasterType.OnUnbind
    );
    
    public static readonly BroadcasterGroup Timers = new(
        EventBroadcasterType.OnCall
    );
    
    public static readonly BroadcasterGroup Tolls = new(
        EventBroadcasterType.OnPay,
        EventBroadcasterType.LoadedPaid
    );

    public readonly EventBroadcasterType[] Types;
    
    private BroadcasterGroup(params EventBroadcasterType[] types)
    {
        Types = types;
    }
}