using System;
using System.Collections.Generic;

namespace Architect.Attributes.Receivers;

public abstract class EventReceiverType
{
    protected abstract EventReceiver Instantiate();

    public EventReceiver Deserialize(Dictionary<string, string> data)
    {
        return Create(data["name"], data.TryGetValue("times", out var value) ? Convert.ToInt32(value) : 1);
    }

    public EventReceiver Create(string name, int times)
    {
        var receiver = Instantiate();
        receiver.EventName = name;
        receiver.RequiredCalls = times;
        return receiver;
    }

    public abstract string GetName();
}

public abstract class EventReceiver
{
    public abstract void ReceiveEvent(EventReceiverInstance instance);

    public string EventName;

    public int RequiredCalls = 1;

    protected abstract EventReceiverType GetReceiverType();

    public Dictionary<string, string> Serialize()
    {
        Dictionary<string, string> data = new()
        {
            ["type"] = GetReceiverType().GetName(),
            ["name"] = EventName
        };
        if (RequiredCalls != 1) data["times"] = RequiredCalls.ToString();
        return data;
    }
}
