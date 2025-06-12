using UnityEngine;

namespace Architect.Attributes.Broadcasters;

public class EventBroadcasterInstance : MonoBehaviour
{
    public string eventName;
    public EventBroadcasterType eventBroadcasterType;

    public EventBroadcasterType GetEventType()
    {
        return eventBroadcasterType;
    }

    public void Broadcast()
    { 
        EventManager.BroadcastEvent(eventName);
    }
}