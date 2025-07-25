using System.Collections.Generic;

namespace Architect.Attributes.Receivers;

public class EventReceiver(string typeName, string name, int requiredCalls)
{
    public readonly string TypeName = typeName.ToLower();
    public readonly string Name = name.ToLower();
    public readonly int RequiredCalls = requiredCalls;
    
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