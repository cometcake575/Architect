using System.Collections.Generic;
using Architect.Attributes.Config;

namespace Architect.Attributes;

public static class ConfigManager
{
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