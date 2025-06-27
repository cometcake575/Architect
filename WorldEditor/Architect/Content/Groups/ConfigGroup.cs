using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes.Config;
using Architect.Content.Elements.Custom;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Elements.Internal.Fixers;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Modding;
using Modding.Utils;
using Satchel;
using UnityEngine;

namespace Architect.Content.Groups;

public class ConfigGroup
{
    private static bool _initialized;
    
    internal static ConfigGroup All;

    internal static ConfigGroup GeoChest;
    
    internal static ConfigGroup Enemies;
    
    internal static ConfigGroup Mantis;
    
    internal static ConfigGroup Twisters;
    
    internal static ConfigGroup WatcherKnights;
    
    internal static ConfigGroup Cocoon;
    
    internal static ConfigGroup Breakable;
    
    internal static ConfigGroup Levers;
    
    internal static ConfigGroup Tolls;
    
    internal static ConfigGroup MovingObjects;
    
    internal static ConfigGroup Tablets;
    
    internal static ConfigGroup Abyss;
    
    internal static ConfigGroup Grub;
    
    internal static ConfigGroup BattleGate;
    
    internal static ConfigGroup Bindings;
    
    internal static ConfigGroup Conveyors;
    
    internal static ConfigGroup RoomClearer;
    
    internal static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
        All = new ConfigGroup(null,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Active", (o, value) =>
            {
                o.SetActive(value.GetValue());
            })));

        GeoChest = new ConfigGroup(All,
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
        
        Enemies = new ConfigGroup(All,
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

        WatcherKnights = new ConfigGroup(Enemies,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Start Awake", (o, value) =>
            {
                if (!value.GetValue()) return;
                o.LocateMyFSM("Black Knight").AddCustomAction("Rest", fsm =>
                {
                    fsm.SendEvent("WAKE");
                });
            }))
        );

        Breakable = new ConfigGroup(All,
            Attributes.ConfigManager.RegisterConfigType(MakePersistenceConfigType("Stay Broken"))
        );

        Cocoon = new ConfigGroup(Breakable,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Lifeseed Count", (o, value) =>
            {
                o.GetComponent<HealthCocoon>().SetScuttlerAmount(Mathf.Abs(value.GetValue()));
            }))
        );

        Levers = new ConfigGroup(All,
            Attributes.ConfigManager.RegisterConfigType(MakePersistenceConfigType("Stay Activated"))
        );

        Tolls = new ConfigGroup(Levers,
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

        MovingObjects = new ConfigGroup(All,
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
        
        ModHooks.LanguageGetHook += (key, title, orig) => title == "Custom" ? CustomTexts[key] : orig;
        Tablets = new ConfigGroup(All,
            Attributes.ConfigManager.RegisterConfigType(new StringConfigType("Content", (o, value) =>
            {
                var fsm = o.LocateMyFSM("Inspection");
                fsm.FsmVariables.GetFsmString("Convo Name").Value = o.name;
                fsm.FsmVariables.GetFsmString("Sheet Name").Value = "Custom";
                CustomTexts[o.name] = value.GetValue();
            }))
        );

        Abyss = new ConfigGroup(All,
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
            All,
            Attributes.ConfigManager.RegisterConfigType(
                new BoolConfigType("Start Opened", (o, value) =>
                {
                    if (!value.GetValue()) return;
                    var fsm = o.LocateMyFSM("BG Control");
                    fsm.SetState("Open");
                })
            )
        );

        Bindings = new ConfigGroup(
            All,
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
            All,
            Attributes.ConfigManager.RegisterConfigType(
                new FloatConfigType("Belt Speed", (o, value) =>
                {
                    o.transform.GetChild(0).GetComponent<ConveyorBelt>().speed = value.GetValue();
                    o.GetComponent<Animator>().speed = value.GetValue() / 8;
                }, true)
            )
        );

        RoomClearer = new ConfigGroup(
            All,
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
    
    private ConfigGroup([CanBeNull] ConfigGroup parent, params ConfigType[] types)
    {
        Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
    }
}