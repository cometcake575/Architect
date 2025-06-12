using System.Collections.Generic;
using Architect.Attributes.Config;
using Architect.Content.Groups;

namespace Architect.Attributes;

public static class ConfigManager
{
    private static bool _initializedForSerialization;
    
    public static void Initialize()
    {
        if (_initializedForSerialization) return;
        _initializedForSerialization = true;
        
        ConfigGroup.Initialize();
    }

    public static ConfigType RegisterConfigType(ConfigType type)
    {
        ConfigTypes[type.Name] = type;
        return type;
    }

    private static readonly Dictionary<string, ConfigType> ConfigTypes = new();

    public static ConfigValue DeserializeConfigValue(string configType, string serializedValue)
    {
        return ConfigTypes[configType].Deserialize(serializedValue);
    }
}