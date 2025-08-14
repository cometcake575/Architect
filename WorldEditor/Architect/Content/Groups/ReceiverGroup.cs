using System.Linq;
using System.Reflection;
using Architect.Attributes;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Elements.Internal.Fixers;
using GlobalEnums;
using JetBrains.Annotations;
using Modding;
using Satchel;
using UnityEngine;

namespace Architect.Content.Groups;

public class ReceiverGroup([CanBeNull] ReceiverGroup parent, params string[] types)
{
    private static bool _initialized;
    
    internal static ReceiverGroup Invisible;
    
    internal static ReceiverGroup Generic;
    
    internal static ReceiverGroup Gates;
    
    internal static ReceiverGroup BattleGate;
    
    internal static ReceiverGroup Enemies;
    
    internal static ReceiverGroup Binoculars;
    
    internal static ReceiverGroup TeleportPoint;
    
    internal static ReceiverGroup HazardRespawnPoint;
    
    internal static ReceiverGroup Stompers;
    
    internal static ReceiverGroup MovingWall;
    
    internal static ReceiverGroup Playable;
    
    internal static ReceiverGroup Interactions;
    
    internal static ReceiverGroup WalkTarget;
    
    internal static ReceiverGroup Transitions;
    
    internal static ReceiverGroup Mov;
    
    internal static ReceiverGroup Awakable;
    
    internal static ReceiverGroup UpDownPlatforms;
    
    internal static ReceiverGroup ColosseumPlatform;
    
    internal static ReceiverGroup Relays;
    
    internal static ReceiverGroup JellyEgg;
    
    internal static ReceiverGroup PlayerHook;
    
    internal static ReceiverGroup TextDisplay;
    
    internal static ReceiverGroup ObjectMover;
    
    internal static ReceiverGroup ObjectDuplicator;
    
    internal static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        Invisible = new ReceiverGroup(null, 
            EventManager.RegisterEventReceiverType("disable", o => o.SetActive(false)),
            EventManager.RegisterEventReceiverType("enable", o => o.SetActive(true))
        );

        Generic = new ReceiverGroup(Invisible, 
            EventManager.RegisterEventReceiverType("hide", o =>
            {
                foreach (var renderer in o.GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            }),
            EventManager.RegisterEventReceiverType("show", o =>
            {
                foreach (var renderer in o.GetComponentsInChildren<Renderer>()) renderer.enabled = true;
            })
        );

        ObjectMover = new ReceiverGroup(Invisible, EventManager.RegisterEventReceiverType("move", o =>
        {
            o.GetComponent<ObjectMover>().DoMove();
        }));

        ObjectDuplicator = new ReceiverGroup(Invisible, EventManager.RegisterEventReceiverType("duplicate", o =>
        {
            o.GetComponent<ObjectDuplicator>().Duplicate();
        }));

        JellyEgg = new ReceiverGroup(Generic, EventManager.RegisterEventReceiverType("respawn_egg", o =>
        {
            var egg = o.GetComponent<JellyEgg>();
            egg.meshRenderer.enabled = true;
            egg.circleCollider.enabled = true;
        }));
        
        Gates = new ReceiverGroup(Generic, EventManager.RegisterEventReceiverType("open", o =>
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

        Enemies = new ReceiverGroup(Generic, EventManager.RegisterEventReceiverType("die", o =>
        {
            if (!o.activeInHierarchy) return;
            o.GetComponent<HealthManager>().Die(null, AttackTypes.Generic, true);
        }));
        
        Binoculars = new ReceiverGroup(Generic, EventManager.RegisterEventReceiverType("start_using", o =>
        {
            if (!o.activeInHierarchy) return;
            o.GetComponent<Binoculars>().StartUsing();
        }));
        
        TeleportPoint = new ReceiverGroup(Invisible, EventManager.RegisterEventReceiverType("teleport", o =>
        {
            HeroController.instance.transform.position = o.transform.position;
        }));
        
        HazardRespawnPoint = new ReceiverGroup(Invisible, EventManager.RegisterEventReceiverType("setspawn", o =>
        {
            PlayerData.instance.SetHazardRespawn(o.GetComponent<HazardRespawnMarker>());
        }));

        Stompers = new ReceiverGroup(Invisible,
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

        MovingWall = new ReceiverGroup(Generic,
            EventManager.RegisterEventReceiverType("in", o =>
            {
                var fsm = o.LocateMyFSM("Control");
                fsm.FsmVariables.FindFsmFloat("Distance").Value = o.GetComponent<CustomWallMover>().moveDistance;
                fsm.SendEvent("MOVE");
            }),
            EventManager.RegisterEventReceiverType("out", o =>
            {
                var fsm = o.LocateMyFSM("Control");
                fsm.FsmVariables.FindFsmFloat("Distance").Value = 0;
                fsm.SendEvent("MOVE");
            })
        );

        Playable = new ReceiverGroup(Invisible, 
            EventManager.RegisterEventReceiverType("play", o =>
            {
                o.GetComponent<Playable>().Play();
            })
        );

        Interactions = new ReceiverGroup(Invisible, 
            EventManager.RegisterEventReceiverType("show_label", o =>
            {
                o.GetComponent<Interaction>().GoUp();
            }), 
            EventManager.RegisterEventReceiverType("hide_label", o =>
            {
                o.GetComponent<Interaction>().GoDown();
            })
        );

        WalkTarget = new ReceiverGroup(Invisible, 
            EventManager.RegisterEventReceiverType("walk", o =>
            {
                o.GetComponent<WalkTarget>().DoWalk();
            })
        );

        var triggerEnter = typeof(TransitionPoint).GetMethod("OnTriggerEnter2D", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        Transitions = new ReceiverGroup(Invisible, 
            EventManager.RegisterEventReceiverType("transition", o =>
            {
                var tp = o.GetComponent<TransitionPoint>();
                
                var wasADoor = tp.isADoor;
                tp.isADoor = false;
                triggerEnter?.Invoke(tp, [HeroController.instance.GetComponent<Collider2D>()]);
                if (wasADoor) tp.isADoor = true;
            })
        );

        Mov = new ReceiverGroup(Playable, 
            EventManager.RegisterEventReceiverType("pause", o =>
            {
                o.GetComponent<MovObject>().Pause();
            })
        );

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

        UpDownPlatforms = new ReceiverGroup(Generic,
            EventManager.RegisterEventReceiverType("up", o =>
            {
                var fsm = o.GetComponent<PlayMakerFSM>();
                fsm.SendEvent("APPEAR");
                fsm.SendEvent("PLAT EXPAND");
            }),
            EventManager.RegisterEventReceiverType("down", o =>
            { 
                var fsm = o.GetComponent<PlayMakerFSM>();
                fsm.SendEvent("DISAPPEAR");
                fsm.SendEvent("PLAT RETRACT");
            })
        );

        ColosseumPlatform = new ReceiverGroup(UpDownPlatforms,
            EventManager.RegisterEventReceiverType("down_slow", o =>
            { 
                o.LocateMyFSM("Control").SendEvent("SLOW RETRACT");
            })
        );

        Relays = new ReceiverGroup(null,
            EventManager.RegisterEventReceiverType("call", o =>
            {
                if (!o.activeInHierarchy) return;
                var relay = o.GetComponent<Relay>();
                relay.DoRelay();
            }),
            EventManager.RegisterEventReceiverType("enable_relay", o =>
            {
                var relay = o.GetComponent<Relay>();
                relay.EnableRelay();
            }),
            EventManager.RegisterEventReceiverType("disable_relay", o =>
            {
                var relay = o.GetComponent<Relay>();
                relay.DisableRelay();
            })
        );

        PlayerHook = new ReceiverGroup(Invisible,
            EventManager.RegisterEventReceiverType("kill_player", o =>
            {
                HeroController.instance.TakeDamage(o, CollisionSide.other, 999, 2);
            }),
            EventManager.RegisterEventReceiverType("damage_player", o =>
            {
                HeroController.instance.TakeDamage(o, CollisionSide.other, 1, 1);
            }),
            EventManager.RegisterEventReceiverType("hazard_player", o =>
            {
                HeroController.instance.TakeDamage(o, CollisionSide.other, 1, 2);
            }),
            EventManager.RegisterEventReceiverType("heal_player", _ =>
            {
                HeroController.instance.AddHealth(1);
            }),
            EventManager.RegisterEventReceiverType("full_heal_player", _ =>
            {
                HeroController.instance.MaxHealth();
            })
        );

        TextDisplay = new ReceiverGroup(Invisible,
            EventManager.RegisterEventReceiverType("display", o =>
            {
                o.GetComponent<TextDisplay>().Display();
            })
        );
    }

    public readonly string[] Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
}