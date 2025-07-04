using System.Linq;
using Architect.Attributes;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Elements.Custom.SaL;
using JetBrains.Annotations;
using Modding;
using Satchel;
using UnityEngine;

namespace Architect.Content.Groups;

public class ReceiverGroup
{
    private static bool _initialized;
    
    internal static ReceiverGroup All;
    
    internal static ReceiverGroup Gates;
    
    internal static ReceiverGroup BattleGate;
    
    internal static ReceiverGroup Enemies;
    
    internal static ReceiverGroup TeleportPoint;
    
    internal static ReceiverGroup HazardRespawnPoint;
    
    internal static ReceiverGroup Stompers;
    
    internal static ReceiverGroup Awakable;
    
    internal static ReceiverGroup Relays;
    
    internal static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        All = new ReceiverGroup(null, 
            EventManager.RegisterEventReceiverType("disable", o => o.SetActive(false)),
            EventManager.RegisterEventReceiverType("enable", o => o.SetActive(true))
        );
        
        Gates = new ReceiverGroup(All, EventManager.RegisterEventReceiverType("open", o =>
        {
            foreach (var fsm in o.GetComponents<PlayMakerFSM>())
            {
                fsm.SendEvent("DOWN");
                fsm.SendEvent("OPEN");
                fsm.SendEvent("DEACTIVATE");
                if (!fsm.TryGetState("Open", out var state)) continue;
                fsm.SetState(state.Name);
            }
        }));
        
        BattleGate = new ReceiverGroup(Gates, EventManager.RegisterEventReceiverType("close", o =>
        {
            var bgControl = o.LocateMyFSM("BG Control");
            if (bgControl) bgControl.SetState("Close 1");
            else o.LocateMyFSM("FSM").SendEvent("UP"); 
        }));

        Enemies = new ReceiverGroup(All, EventManager.RegisterEventReceiverType("die", o =>
        {
            o.GetComponent<HealthManager>().Die(null, AttackTypes.Generic, true);
        }));
        
        TeleportPoint = new ReceiverGroup(All, EventManager.RegisterEventReceiverType("teleport", o =>
        {
            HeroController.instance.transform.position = o.transform.position;
        }));
        
        HazardRespawnPoint = new ReceiverGroup(All, EventManager.RegisterEventReceiverType("setspawn", o =>
        {
            PlayerData.instance.SetHazardRespawn(o.GetComponent<HazardRespawnMarker>());
        }));

        Stompers = new ReceiverGroup(All,
            EventManager.RegisterEventReceiverType("start", o =>
            {
                ReflectionHelper.SetFieldSafe(o.GetComponentInChildren<StopAnimatorsAtPoint>(), "shouldStop", false);
                o.GetComponentInChildren<Animator>().enabled = true;
            }),
            EventManager.RegisterEventReceiverType("stopinstant", o =>
            {
                o.GetComponentInChildren<StopAnimatorsAtPoint>().stopInstantEvent.ReceiveEvent();
            }),
            EventManager.RegisterEventReceiverType("stop", o =>
            {
                o.GetComponentInChildren<StopAnimatorsAtPoint>().stopEvent.ReceiveEvent();
            }));

        Awakable = new ReceiverGroup(Enemies,
            EventManager.RegisterEventReceiverType("wake", o =>
            { 
                foreach (var fsm in o.GetComponents<PlayMakerFSM>())
                {
                    fsm.SendEvent("HERO ENTER");
                    fsm.SendEvent("TAKE DAMAGE");
                    fsm.SendEvent("WAKE");
                }
            })
        );

        Relays = new ReceiverGroup(Enemies,
            EventManager.RegisterEventReceiverType("call", o =>
            {
                if (!o.activeInHierarchy) return;
                var relay = o.GetComponent<Relay>();
                if (!relay.canCall) return;
                relay.canCall = false;
                EventManager.BroadcastEvent(o, "OnCall");
            })
        );
    }

    public readonly string[] Types;
    
    public ReceiverGroup([CanBeNull] ReceiverGroup parent, params string[] types)
    {
        Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
    }
}