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

    public readonly EventBroadcasterType[] Types;
    
    private BroadcasterGroup(params EventBroadcasterType[] types)
    {
        Types = types;
    }
}