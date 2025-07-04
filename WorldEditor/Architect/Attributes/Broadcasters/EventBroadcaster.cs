using System.Collections.Generic;

namespace Architect.Attributes.Broadcasters;

public class EventBroadcaster
{
    public readonly string EventBroadcasterType;
    public readonly string EventName;

    public EventBroadcaster(string type, string name)
    {
        EventBroadcasterType = type;
        EventName = name;
    }

    public Dictionary<string, string> Serialize()
    {
        return new Dictionary<string, string>
        {
            ["type"] = EventBroadcasterType,
            ["name"] = EventName
        };
    }
}