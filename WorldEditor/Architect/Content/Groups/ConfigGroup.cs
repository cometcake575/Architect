using System.Linq;
using Architect.Attributes.Config;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Content.Groups;

public class ConfigGroup
{
    internal static ConfigGroup All;

    internal static ConfigGroup Enemies;
    
    internal static ConfigGroup Cocoon;
    
    internal static ConfigGroup Breakable;
    
    internal static ConfigGroup Levers;

    internal static void Initialize()
    {
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
    }

    public readonly ConfigType[] Types;
    
    private ConfigGroup([CanBeNull] ConfigGroup parent, params ConfigType[] types)
    {
        Types = parent != null ? types.Concat(parent.Types).ToArray() : types;
    }
}