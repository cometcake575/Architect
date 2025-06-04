using System.Collections.Generic;
using Modding.Converters;
using Newtonsoft.Json;

namespace Architect.utils;

public class WorldEditorGlobalSettings
{
    public List<string> Favourites = new();

    public Dictionary<string, bool> ContentPackSettings = new();

    public bool CanEnableEditing = false;
    
    [JsonConverter(typeof(PlayerActionSetConverter))]
    public readonly WorldEditorKeyBinds Keybinds = new();

    public Dictionary<string, Dictionary<string, List<Dictionary<string, string>>>> Edits = new();
}