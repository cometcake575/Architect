using System.Linq;
using Architect.Attributes;
using Architect.Attributes.Receivers;
using JetBrains.Annotations;
using Satchel;

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
    
    //internal static ReceiverGroup WatcherKnights;
    
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
                if (!fsm.TryGetState("Open", out var state)) continue;
                fsm.SetState(state.Name);
            }
        }));
        
        BattleGate = new ReceiverGroup(Gates, EventManager.RegisterEventReceiverType("close", o =>
        {
            o.LocateMyFSM("BG Control").SetState("Close 1");
        }));

        Enemies = new ReceiverGroup(All, EventManager.RegisterEventReceiverType("die", o =>
        {
            o.GetComponent<HealthManager>().Die(null, AttackTypes.Generic, true);
        }));
        
        TeleportPoint = new ReceiverGroup(All, EventManager.RegisterEventReceiverType("teleport", o =>
        {
            HeroController.instance.transform.position = o.transform.position;
        }));
        
        HazardRespawnPoint = new ReceiverGroup(All, EventManager.RegisterEventReceiverType("set_spawn", o =>
        {
            PlayerData.instance.SetHazardRespawn(o.GetComponent<HazardRespawnMarker>());
        }));
    }

    public readonly string[] Types;
    
    private ReceiverGroup([CanBeNull] ReceiverGroup parent, params string[] types)
    {
        Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
    }
}