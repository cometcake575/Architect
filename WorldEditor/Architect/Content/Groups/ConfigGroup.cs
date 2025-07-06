using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes.Config;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Elements.Custom.SaL;
using Architect.Content.Elements.Internal.Fixers;
using HutongGames.PlayMaker.Actions;
using Modding;
using Modding.Utils;
using Satchel;
using UnityEngine;

namespace Architect.Content.Groups;

public class ConfigGroup
{
    private static bool _initialized;
    
    public static ConfigGroup Invisible;
    
    public static ConfigGroup Generic;

    public static ConfigGroup GeoChest;
    
    public static ConfigGroup KeyListeners;
    
    public static ConfigGroup Enemies;
    
    public static ConfigGroup Mantis;
    
    public static ConfigGroup Twisters;
    
    public static ConfigGroup Awakable;
    
    public static ConfigGroup GruzMother;
    
    public static ConfigGroup VengeflyKing;
    
    public static ConfigGroup Cocoon;
    
    public static ConfigGroup Breakable;
    
    public static ConfigGroup Levers;
    
    public static ConfigGroup TollBench;
    
    public static ConfigGroup Toll;
    
    public static ConfigGroup MovingObjects;
    
    public static ConfigGroup Tablets;
    
    public static ConfigGroup Relays;
    
    public static ConfigGroup Abyss;
    
    public static ConfigGroup VisibleAbyss;
    
    public static ConfigGroup Grub;
    
    public static ConfigGroup BattleGate;
    
    public static ConfigGroup BrokenVessel;
    
    public static ConfigGroup Bindings;
    
    public static ConfigGroup Conveyors;
    
    public static ConfigGroup RoomClearer;
    
    public static ConfigGroup ObjectRemover;
    
    public static ConfigGroup HazardRespawnPoint;
    
    public static ConfigGroup RepeatNpcs;
    
    public static ConfigGroup Midwife;
    
    public static ConfigGroup Timers;
    
    public static ConfigGroup Decorations;
    
    public static ConfigGroup Stretchable;
    
    public static ConfigGroup Colours;
    
    public static ConfigGroup Shapes;
    
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
        Invisible = new ConfigGroup(Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Active", (o, value) =>
            {
                o.SetActive(value.GetValue());
            }))
        );
        
        Generic = new ConfigGroup(Invisible,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Visible", (o, value) =>
            {
                foreach (var renderer in o.GetComponentsInChildren<Renderer>()) renderer.enabled = value.GetValue();
            }))
        );

        GeoChest = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Large Geo", (o, value) =>
            {
                o.LocateMyFSM("Chest Control").FsmVariables.FindFsmInt("Geo Large").Value = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Medium Geo", (o, value) =>
            {
                o.LocateMyFSM("Chest Control").FsmVariables.FindFsmInt("Geo Med").Value = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Small Geo", (o, value) =>
            {
                o.LocateMyFSM("Chest Control").FsmVariables.FindFsmInt("Geo Small").Value = value.GetValue();
            }))
        );
        
        Enemies = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Health", (o, value) =>
            {
                o.GetComponent<HealthManager>().hp = Mathf.Abs(value.GetValue());
            })),

            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Large Geo Drops", (o, value) =>
            {
                o.GetComponent<HealthManager>().SetGeoLarge(Mathf.Abs(value.GetValue()));
            })),

            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Medium Geo Drops", (o, value) =>
            {
                o.GetComponent<HealthManager>().SetGeoMedium(Mathf.Abs(value.GetValue()));
            })),

            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Small Geo Drops", (o, value) =>
            {
                o.GetComponent<HealthManager>().SetGeoSmall(Mathf.Abs(value.GetValue()));
            })),

            Attributes.ConfigManager.RegisterConfigType(MakePersistenceConfigType("Stay Dead", (o) =>
            {
                var item = o.GetComponent<PersistentBoolItem>();
                item.OnSetSaveState += b =>
                {
                    o.GetComponent<HealthManager>().isDead = b;
                };
                item.OnGetSaveState += (ref bool b) =>
                {
                    b = o.GetComponent<HealthManager>().isDead;
                };
            }))
        );

        Mantis = new ConfigGroup(Enemies,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Ignore Respect", (o, value) =>
            {
                if (!value.GetValue()) return;
                var mantis = o.LocateMyFSM("Mantis");
                if (mantis)
                {
                    var defeated = mantis.GetAction<PlayerDataBoolTest>("Lords Defeated?", 0);
                    defeated.isTrue = defeated.isFalse;
                }
                var child = o.LocateMyFSM("Mantis Flyer");
                if (child)
                {
                    child.SendEvent("TOOK DAMAGE");
                }
            }))
        );

        Twisters = new ConfigGroup(Enemies,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Teleplane Width", (o, value) =>
            {
                var collider = o.GetComponent<Teleplane>().collider;
                collider.size = new Vector2(value.GetValue(), collider.size.y);
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Teleplane Height", (o, value) =>
            {
                var collider = o.GetComponent<Teleplane>().collider;
                collider.size = new Vector2(collider.size.x, value.GetValue());
            }))
        );

        Awakable = new ConfigGroup(Enemies,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Start Awake", (o, value) =>
            {
                if (!value.GetValue()) return;
                var bigFly = o.LocateMyFSM("Big Fly Control");
                if (bigFly)
                {
                    bigFly.AddCustomAction("Init", _ => { bigFly.SendEvent("GG BOSS"); });
                    return;
                }
                
                o.LocateMyFSM("Black Knight").AddCustomAction("Rest", fsm => fsm.SendEvent("WAKE"));
            }))
        );

        GruzMother = new ConfigGroup(Awakable,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Spawn Gruzzers", (o, value) =>
            {
                if (value.GetValue()) return;
                o.GetComponent<GruzMotherElement.GmConfig>().spawnGruzzers = false;
            }, true))
        );

        VengeflyKing = new ConfigGroup(Awakable,
            Attributes.ConfigManager.RegisterConfigType(new ChoiceConfigType("Dive Height", (o, value) =>
            {
                if (value.GetValue() == 1) return;
                o.GetComponent<VengeflyKingElement.VkConfig>().targetPlayer = false;
            }, true, "Vanilla", "Player")),
            Attributes.ConfigManager.RegisterConfigType(new ChoiceConfigType("Vengeflies", (o, value) =>
            {
                o.GetComponent<VengeflyKingElement.VkConfig>().vengeflyRule = value.GetValue();
            }, true, "Off", "Vanilla", "Local"))
        );

        Breakable = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(MakePersistenceConfigType("Stay Broken"))
        );

        Cocoon = new ConfigGroup(Breakable,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Lifeseed Count", (o, value) =>
            {
                o.GetComponent<HealthCocoon>().SetScuttlerAmount(Mathf.Abs(value.GetValue()));
            }))
        );

        Levers = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(MakePersistenceConfigType("Stay Activated"))
        );

        TollBench = new ConfigGroup(Levers,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Cost", (o, value) =>
            {
                foreach (var fsm in o.GetComponents<PlayMakerFSM>())
                {
                    if (!fsm.TryGetState("Get Price", out var state)) continue;
                    state.InsertAction(new SetIntValue
                    {
                        intVariable = fsm.FsmVariables.GetFsmInt("Toll Cost"),
                        intValue = value.GetValue()
                    }, 2);
                }
            }))
        );

        Toll = new ConfigGroup(TollBench,
            
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("Text", (o, value) =>
            {
                var fsm = o.LocateMyFSM("Toll Machine");
                
                var id = "Custom Toll Text " + o.name;
                CustomTexts[id] = value.GetValue();
                
                fsm.InsertAction("Send Text", new SetStringValue
                {
                    stringVariable = fsm.FsmVariables.GetFsmString("Prompt Convo"),
                    stringValue = id
                }, 2);
            }))
        );

        MovingObjects = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Track Distance", (o, value) =>
            {
                o.GetOrAddComponent<MovingObject>().trackDistance = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Speed", (o, value) =>
            {
                o.GetOrAddComponent<MovingObject>().speed = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Pause Time", (o, value) =>
            {
                o.GetOrAddComponent<MovingObject>().pauseTime = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Start Offset", (o, value) =>
            {
                o.GetOrAddComponent<MovingObject>().offset = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Track Rotation", (o, value) =>
            {
                o.GetOrAddComponent<MovingObject>().rotation = value.GetValue();
            }))
        );
        
        ModHooks.LanguageGetHook += (key, _, orig) => CustomTexts.TryGetValue(key, out var customText) ? customText : orig;

        Tablets = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("Content", (o, value) =>
            {
                var fsm = o.LocateMyFSM("Inspection");
                var id = "Custom Tablet " + o.name;
                
                fsm.FsmVariables.GetFsmString("Convo Name").Value = id;
                fsm.FsmVariables.GetFsmString("Sheet Name").Value = "Custom";
                CustomTexts[id] = value.GetValue();
            }))
        );

        Abyss = new ConfigGroup(Invisible,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Ignore Void Heart", (o, value) =>
                {
                    if (value.GetValue()) o.GetComponent<VhEffects>().ForceDisable();
                })
            )
        );

        VisibleAbyss = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Ignore Void Heart", (o, value) =>
                {
                    if (value.GetValue()) o.GetComponent<VhEffects>().ForceDisable();
                })
            )
        );

        Grub = new ConfigGroup(Breakable,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Contains Grub", (o, value) =>
                {
                    if (!value.GetValue()) o.transform.GetChild(1).gameObject.SetActive(false);
                })
            )
        );

        BattleGate = new ConfigGroup(
            Generic,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Start Opened", (o, value) =>
                {
                    if (!value.GetValue()) return;
                    var fsm = o.LocateMyFSM("BG Control");
                    if (fsm) fsm.SetState("Open");
                    else o.LocateMyFSM("FSM").SendEvent("DOWN");
                })
            )
        );

        Bindings = new ConfigGroup(
            Generic,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Binding Active", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetComponent<CustomBinder>().active = false;
                })
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Reversible", (o, value) =>
                {
                    if (!value.GetValue()) return;
                    o.GetComponent<CustomBinder>().reversible = true;
                })
            )
        );

        Conveyors = new ConfigGroup(
            Generic,
            Attributes.ConfigManager.RegisterConfigType(
                new FloatConfigType("Belt Speed", (o, value) =>
                {
                    o.transform.GetChild(0).GetComponent<ConveyorBelt>().speed = value.GetValue();
                    o.GetComponent<Animator>().speed = value.GetValue() / 8;
                }, true)
            )
        );

        RoomClearer = new ConfigGroup(
            Invisible,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Transitions", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeTransitions = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Benches", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeBenches = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Props", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeProps = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Scenery", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeScenery = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Blur", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeBlur = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Tilemap", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeTilemap = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove NPCs", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeNpcs = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Camera Lock", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeCameraLocks = false;
                }, true)
            ),
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Remove Music", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.GetOrAddComponent<RoomClearerConfig>().removeMusic = false;
                }, true)
            )
        );

        ObjectRemover = new ConfigGroup(
            Invisible,
            Attributes.ConfigManager.RegisterConfigType(
                new StringConfigType("Path", (o, value) =>
                {
                    o.GetOrAddComponent<ObjectRemoverConfig>().objectPath = value.GetValue();
                }, true)
            )
        );

        HazardRespawnPoint = new ConfigGroup(
            Invisible,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Contact Trigger", (o, value) =>
                {
                    if (value.GetValue()) return;
                    o.RemoveComponent<Collider2D>();
                })
            )
        );

        RepeatNpcs = new ConfigGroup(
            Generic,
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("First Convo", (o, value) =>
            {
                CustomTexts[o.GetComponent<NpcEditor>().SetFirstConvo()] = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("Repeat Convo", (o, value) =>
            {
                CustomTexts[o.GetComponent<NpcEditor>().SetRepeatConvo()] = value.GetValue();
            }))
        );

        Midwife = new ConfigGroup(
            Generic,
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("Convo Text", (o, value) =>
            {
                var fsm = o.transform.GetChild(1).gameObject.LocateMyFSM("Conversation Control");
                var id = "Custom Midwife " + o.name;
                
                fsm.DisableAction("Convo Choice", 1);
                fsm.DisableAction("Convo Choice", 2);
                fsm.DisableAction("Convo Choice", 3);
                fsm.DisableAction("Convo Choice", 4);

                fsm.GetAction<CallMethodProper>("Repeat", 0).parameters[0].stringValue = id;
                CustomTexts[id] = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Start Angered", (o, value) =>
            {
                if (value.GetValue()) o.transform.GetChild(1).gameObject.LocateMyFSM("Conversation Control").AddCustomAction("Idle", fsm =>
                {
                    fsm.SetState("Talk Finish");
                });
            }))
        );

        Timers = new ConfigGroup(
            Invisible,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Start Delay", (o, value) =>
            {
                o.GetComponent<Timer>().startDelay = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Repeat Delay", (o, value) =>
            {
                o.GetComponent<Timer>().repeatDelay = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Max Calls", (o, value) =>
            {
                o.GetComponent<Timer>().maxCalls = value.GetValue();
            }))
        );

        KeyListeners = new ConfigGroup(
            Invisible,
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("Key", (o, value) =>
            {
                if (!Enum.TryParse<KeyCode>(value.GetValue(), true, out var key)) return;
                o.GetComponent<KeyListener>().key = key;
            })),
            Attributes.ConfigManager.RegisterConfigType(new ChoiceConfigType("Type", (o, value) =>
            {
                o.GetComponent<KeyListener>().listenMode = value.GetValue();
            }, false, "Press", "Release", "Hold"))
        );

        Decorations = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Z Offset", (o, value) =>
            {
                o.transform.SetPositionZ(o.transform.GetPositionZ() + value.GetValue());
            }))
        );

        Stretchable = new ConfigGroup(Invisible,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("width", (o, value) =>
            {
                o.transform.SetScaleX(o.transform.GetScaleX() * value.GetValue());
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("height", (o, value) =>
            {
                o.transform.SetScaleY(o.transform.GetScaleY() * value.GetValue());
            }))
        );

        Colours = new ConfigGroup(Invisible,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("r", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.r = value.GetValue();
                sr.color = color;
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("g", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.g = value.GetValue();
                sr.color = color;
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("b", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.b = value.GetValue();
                sr.color = color;
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("a", (o, value) =>
            {
                var sr = o.GetComponent<SpriteRenderer>();
                var color = sr.color;
                color.a = value.GetValue();
                sr.color = color;
            })),
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("layer", (o, value) =>
            {
                o.GetComponent<SpriteRenderer>().sortingOrder = value.GetValue();
            }))
        );

        Shapes = new ConfigGroup([Colours, Stretchable],
            Attributes.ConfigManager.RegisterConfigType(new ChoiceConfigType("collision", (o, value) =>
            {
                switch (value.GetValue())
                {
                    case 0:
                        o.RemoveComponent<Collider2D>();
                        break;
                    case 1:
                        o.AddComponent<CustomDamager>().damageAmount = 1;
                        break;
                    case 2:
                        o.GetComponent<Collider2D>().isTrigger = false;
                        o.layer = 8;
                        break;
                }
            }, false, "None", "Hazard", "Solid"))
        );

        Relays = new ConfigGroup(Generic,
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("Relay ID", (o, value) =>
            {
                o.GetComponent<Relay>().id = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Reset on Bench", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.GetComponent<Relay>().semiPersistent = true;
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Relay Chance", (o, value) =>
            {
                o.GetComponent<Relay>().relayChance = value.GetValue();
            }))
        );

        BrokenVessel = new ConfigGroup(Enemies,
            Attributes.ConfigManager.RegisterConfigType(new ChoiceConfigType("Jump Zone", (o, value) =>
            {
                if (value.GetValue() != 1) return;
                o.GetComponent<BrokenVesselElement.BrokenVesselConfig>().localJump = false;
            }, true, "Local", "Arena")),
            Attributes.ConfigManager.RegisterConfigType(new ChoiceConfigType("Infected Balloons", (o, value) =>
            {
                o.GetComponent<BrokenVesselElement.BrokenVesselConfig>().balloons = value.GetValue();
            }, true, "Local", "Arena", "Disabled")),
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Disable Roar", (o, value) =>
            {
                if (value.GetValue()) return;
                o.GetComponent<BrokenVesselElement.BrokenVesselConfig>().disableRoar = false;
            }, true))
        );
        
        SaLGroups.InitializeConfig();
    }

    public static ConfigType MakePersistenceConfigType(string name, Action<GameObject> action = null)
    {
        return new ChoiceConfigType(name, (o, value) =>
        {
            var val = value.GetValue();

            if (val == 0) o.RemoveComponent<PersistentBoolItem>();
            else
            {
                var item = o.GetComponent<PersistentBoolItem>();

                if (!item)
                {
                    item = o.AddComponent<PersistentBoolItem>();
                    item.persistentBoolData = new PersistentBoolData
                    {
                        id = o.name,
                        sceneName = o.scene.name
                    };
                    item.enabled = true;
                    action?.Invoke(o);
                }
                
                item.semiPersistent = val == 1;
                item.persistentBoolData.semiPersistent = val == 1;
            }
        }, true, "False", "Bench", "True");
    }

    private static readonly Dictionary<string, string> CustomTexts = new();

    public readonly ConfigType[] Types;

    public ConfigGroup(ConfigGroup[] parents, params ConfigType[] types)
    {
        Types = types;
        foreach (var parent in parents)
        {
            Types = Types.Concat(parent.Types).ToArray();
        }
    }

    public ConfigGroup(ConfigGroup parent, params ConfigType[] types)
    {
        Types = types.Concat(parent.Types).ToArray();
    }

    public ConfigGroup(params ConfigType[] types)
    {
        Types = types;
    }
}