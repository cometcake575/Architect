using System.Linq;
using Architect.Attributes.Config;
using Architect.Content.Elements.Custom;
using JetBrains.Annotations;
using Modding.Utils;
using UnityEngine;

namespace Architect.Content.Groups;

public class ConfigGroup
{
    private static bool _initialized;
    
    internal static ConfigGroup All;

    internal static ConfigGroup Enemies;
    
    internal static ConfigGroup Cocoon;
    
    internal static ConfigGroup Breakable;
    
    internal static ConfigGroup Levers;
    
    internal static ConfigGroup Sawblade;

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

            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Stay Dead", (o, value) =>
            {
                o.GetComponent<PersistentBoolItem>().persistentBoolData.semiPersistent = !value.GetValue();
                o.GetComponent<PersistentBoolItem>().semiPersistent = !value.GetValue();
            }, o => o.GetComponent<PersistentBoolItem>()))
        );

        Breakable = new ConfigGroup(All,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Stay Broken", (o, value) =>
            {
                o.GetComponent<PersistentBoolItem>().persistentBoolData.semiPersistent = !value.GetValue();
                o.GetComponent<PersistentBoolItem>().semiPersistent = !value.GetValue();
            }))
        );

        Cocoon = new ConfigGroup(Breakable,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Lifeseed Count", (o, value) =>
            {
                o.GetComponent<HealthCocoon>().SetScuttlerAmount(Mathf.Abs(value.GetValue()));
            }))
        );

        Levers = new ConfigGroup(All,
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Stay Pulled", (o, value) =>
            {
                o.GetComponent<PersistentBoolItem>().persistentBoolData.semiPersistent = !value.GetValue();
                o.GetComponent<PersistentBoolItem>().semiPersistent = !value.GetValue();
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
    }

    public readonly ConfigType[] Types;
    
    private ConfigGroup([CanBeNull] ConfigGroup parent, params ConfigType[] types)
    {
        Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
    }
}