using System;
using Architect.Attributes;
using Architect.Content.Elements.Internal.Fixers;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class TriggerZone : MonoBehaviour
{
    public int mode;

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (mode)
        {
            case 0:
                if (!other.gameObject.GetComponent<HeroController>()) return;
                break;
            case 1:
                if (!other.gameObject.GetComponent<NailSlash>()) return;
                break;
            case 2:
                if (!other.gameObject.GetComponent<HealthManager>()) return;
                break;
            case 3:
                if (!other.gameObject.GetComponent<ZoteHead>()) return;
                break;
        }
        EventManager.BroadcastEvent(gameObject, "ZoneEnter");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        switch (mode)
        {
            case 0:
                if (!other.gameObject.GetComponent<HeroController>()) return;
                break;
            case 1:
                if (!other.gameObject.GetComponent<NailSlash>()) return;
                break;
            case 2:
                if (!other.gameObject.GetComponent<HealthManager>()) return;
                break;
            case 3:
                if (!other.gameObject.GetComponent<ZoteHead>()) return;
                break;
        }
        EventManager.BroadcastEvent(gameObject, "ZoneExit");
    }
}