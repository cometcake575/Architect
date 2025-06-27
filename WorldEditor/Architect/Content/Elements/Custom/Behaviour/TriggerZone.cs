using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class TriggerZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<HeroController>()) return;
        EventManager.BroadcastEvent(gameObject, EventBroadcasterType.ZoneEnter);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.GetComponent<HeroController>()) return;
        EventManager.BroadcastEvent(gameObject, EventBroadcasterType.ZoneExit);
    }
}