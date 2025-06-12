using System.Collections.Generic;

namespace Architect.Attributes.Broadcasters;

public class EventBroadcaster
{
    public readonly EventBroadcasterType EventBroadcasterType;
    public readonly string EventName;

    public EventBroadcaster(EventBroadcasterType type, string name)
    {
        EventBroadcasterType = type;
        EventName = name;
    }

    public Dictionary<string, string> Serialize()
    {
        return new Dictionary<string, string>
        {
            ["type"] = EventBroadcasterType.ToString(),
            ["name"] = EventName
        };
    }
}