using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes.Broadcasters;
using Architect.Attributes.Receivers;
using Architect.MultiplayerHook;
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
            BroadcastEvent(effects.gameObject, "OnDeath");
        };

        On.HealthManager.TakeDamage += (orig, self, instance) =>
        {
            BroadcastEvent(self.gameObject, "OnDamage");
            orig(self, instance);
        };

        On.Breakable.Break += (orig, self, min, max, multiplier) =>
        {
            orig(self, min, max, multiplier);
            BroadcastEvent(self.gameObject, "OnBreak");
        };
        
        On.HealthManager.Awake += (orig, self) =>
        {
            orig(self);

            var component = self.GetComponent<PersistentBoolItem>();
            if (!component) return;
            component.OnSetSaveState += value =>
            {
                if (value) BroadcastEvent(self.gameObject, "LoadedDead");
            };
        };
    }

    public static string RegisterEventReceiverType(string name, Action<GameObject> action)
    {
        EventReceiverTypes[name] = action;
        return name;
    }
    
    public static void RunEvent(string eventName, GameObject gameObject) {
        EventReceiverTypes[eventName].Invoke(gameObject);
    }

    private static readonly Dictionary<string, Action<GameObject>> EventReceiverTypes = new();
    
    private static readonly Dictionary<string, List<EventReceiverInstance>> Events = new();

    public static void RegisterEventReceiver(string name, EventReceiverInstance receiver)
    {
        if (!Events.ContainsKey(name)) Events[name] = [];
        Events[name].Add(receiver);
    }

    public static void ClearEventReceivers()
    {
        Events.Clear();
    }

    public static EventReceiver DeserializeReceiver(Dictionary<string, string> data)
    {
        return new EventReceiver(data["type"], data["name"],
            data.TryGetValue("times", out var value) ? Convert.ToInt32(value) : 1);
    }

    public static EventBroadcaster DeserializeBroadcaster(Dictionary<string, string> data)
    {
        return new EventBroadcaster(data["type"], data["name"]);
    }

    public static void BroadcastEvent(GameObject gameObject, string type, bool multiplayer = false)
    {
        var broadcasters = gameObject.GetComponents<EventBroadcasterInstance>();
            
        foreach (var broadcaster in broadcasters)
        {
            if (broadcaster.GetEventType().Equals(type, StringComparison.InvariantCultureIgnoreCase)) broadcaster.Broadcast(multiplayer);
        }
    }
    
    public static void BroadcastEvent(string name, bool multiplayer = false)
    {
        if (multiplayer && Architect.UsingMultiplayer) HkmpHook.BroadcastEvent(name);
        if (!Events.TryGetValue(name, out var @event)) return;
        foreach (var receiver in @event.Where(receiver => receiver && receiver.gameObject))
        {
            receiver.ReceiveEvent();
        }
    }
}