using System.Collections.Generic;

namespace Architect.Attributes.Broadcasters;

public class EventBroadcaster(string type, string name)
{
    public readonly string EventBroadcasterType = type;
    public readonly string EventName = name.ToLower();

    public Dictionary<string, string> Serialize()
    {
        return new Dictionary<string, string>
        {
            ["type"] = EventBroadcasterType,
            ["name"] = EventName
        };
    }
}