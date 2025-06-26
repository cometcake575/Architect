using System.Collections.Generic;

namespace Architect.Attributes.Receivers;

public record EventReceiver(string TypeName, string Name, int RequiredCalls)
{
    public Dictionary<string, string> Serialize()
    {
        Dictionary<string, string> data = new()
        {
            ["type"] = TypeName,
            ["name"] = Name
        };
        if (RequiredCalls != 1) data["times"] = RequiredCalls.ToString();
        return data;
    }
}