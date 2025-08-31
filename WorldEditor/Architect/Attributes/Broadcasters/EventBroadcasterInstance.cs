using UnityEngine;

namespace Architect.Attributes.Broadcasters;

public class EventBroadcasterInstance : MonoBehaviour
{
    public string eventName;
    public string eventBroadcasterType;

    public string GetEventType()
    {
        return eventBroadcasterType;
    }

    public void Broadcast(bool multiplayer)
    {
        EventManager.BroadcastEvent(eventName, multiplayer);
    }
}