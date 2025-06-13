using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Receivers;
using Architect.Content.Groups;
using Modding;
using UnityEngine;

namespace Architect.Attributes;

public static class EventManager
{
    // Called on Initialize as broadcaster types are an enum and do not require initialization for deserialization
    public static void InitializeBroadcasters()
    {
        ModHooks.OnReceiveDeathEventHook += (EnemyDeathEffects effects, bool received, ref float? _,
            ref bool _, ref bool _, ref bool _) =>
        {
            if (received) return;
            BroadcastEvent(effects.gameObject, EventBroadcasterType.OnDeath);
        };
        ModHooks.OnEnableEnemyHook += (enemy, dead) =>
        {
            Architect.Instance.Log("!!!");
            Architect.Instance.Log(enemy + " is " + dead + " dead");
            return dead;
        };
    }

    public static EventReceiverType RegisterEventReceiverType(EventReceiverType eventReceiverType)
    {
        EventReceiverTypes[eventReceiverType.GetName()] = eventReceiverType;
        return eventReceiverType;
    }

    private static readonly Dictionary<string, EventReceiverType> EventReceiverTypes = new();
    
    private static readonly Dictionary<string, List<EventReceiverInstance>> Events = new();

    public static void RegisterEventReceiver(string name, EventReceiverInstance receiver)
    {
        if (!Events.ContainsKey(name)) Events[name] = new List<EventReceiverInstance>();
        Events[name].Add(receiver);
    }

    public static void ClearEventReceivers()
    {
        Events.Clear();
    }

    public static EventReceiver DeserializeReceiver(Dictionary<string, string> data)
    {
        return EventReceiverTypes[data["type"]].Deserialize(data);
    }

    public static EventReceiver CreateReceiver(string type, string name, int times)
    {
        return EventReceiverTypes[type].Create(name, times);
    }

    public static EventBroadcaster DeserializeBroadcaster(Dictionary<string, string> data)
    {
        return !Enum.TryParse(data["type"], out EventBroadcasterType @case) ? null : new EventBroadcaster(@case, data["name"]);
    }

    public static void BroadcastEvent(GameObject gameObject, EventBroadcasterType type)
    {
        var broadcasters = gameObject.GetComponents<EventBroadcasterInstance>();
            
        foreach (var broadcaster in broadcasters)
        {
            if (broadcaster.GetEventType().Equals(type)) broadcaster.Broadcast();
        }
    }
    
    public static void BroadcastEvent(string name)
    {
        if (!Events.TryGetValue(name, out var @event)) return;
        foreach (var receiver in @event)
        {
            receiver.ReceiveEvent();
        }
    }
}