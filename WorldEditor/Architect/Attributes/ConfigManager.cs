using System.Collections.Generic;
using Architect.Attributes.Config;
using Architect.Util;

namespace Architect.Attributes;

public static class ConfigManager
{
    private static readonly Dictionary<string, ConfigType> LegacyConfigTypes = new();
    private static readonly Dictionary<string, ConfigType> ConfigTypes = new();

    public static ConfigType RegisterConfigType(ConfigType type, string id)
    {
        LegacyConfigTypes[type.Name] = type;
        ConfigTypes[id] = type;

        type.Id = id;
        return type;
    }

    public static ConfigValue DeserializeConfigValue(string configType, string serializedValue)
    {
        configType = Updater.UpdateConfig(configType);

        return ConfigTypes.TryGetValue(configType, out var val)
            ? val.Deserialize(serializedValue)
            : LegacyConfigTypes[configType].Deserialize(serializedValue);
    }
}