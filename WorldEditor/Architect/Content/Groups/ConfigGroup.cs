using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Attributes.Config;
using Architect.Content.Elements.Custom;
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

    internal static ConfigGroup Enemies;
    
    internal static ConfigGroup Twisters;
    
    internal static ConfigGroup Cocoon;
    
    internal static ConfigGroup Breakable;
    
    internal static ConfigGroup Levers;
    
    internal static ConfigGroup Tolls;
    
    internal static ConfigGroup Sawblade;
    
    internal static ConfigGroup Tablets;
    
    internal static ConfigGroup Abyss;

    internal static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;
        
        All = new ConfigGroup(null,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Active", (o, value) =>
            {
                o.SetActive(value.GetValue());
            })));
        
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

        Sawblade = new ConfigGroup(All,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Track Distance", (o, value) =>
            {
                o.GetOrAddComponent<MovingSawblade>().trackDistance = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Speed", (o, value) =>
            {
                o.GetOrAddComponent<MovingSawblade>().speed = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Pause Time", (o, value) =>
            {
                o.GetOrAddComponent<MovingSawblade>().pauseTime = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Start Offset", (o, value) =>
            {
                o.GetOrAddComponent<MovingSawblade>().offset = value.GetValue();
            })),
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Track Rotation", (o, value) =>
            {
                o.GetOrAddComponent<MovingSawblade>().rotation = value.GetValue();
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