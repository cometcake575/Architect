using System.Collections.Generic;
using Architect.Attributes.Config;

namespace Architect.Attributes;

public static class ConfigManager
{
    public static ConfigType RegisterConfigType(ConfigType type, string id)
    {
        LegacyConfigTypes[type.Name] = type;
        ConfigTypes[id] = type;

        type.Id = id;
        return type;
    }

    private static readonly Dictionary<string, ConfigType> LegacyConfigTypes = new();
    private static readonly Dictionary<string, ConfigType> ConfigTypes = new();

    public static ConfigValue DeserializeConfigValue(string configType, string serializedValue)
    {
        return ConfigTypes.TryGetValue(configType, out var val) ? 
            val.Deserialize(serializedValue) : 
            LegacyConfigTypes[configType].Deserialize(serializedValue);
    }
}