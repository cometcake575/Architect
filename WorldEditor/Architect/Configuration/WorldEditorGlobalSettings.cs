using System.Collections.Generic;
using Architect.Util;
using Modding.Converters;
using Newtonsoft.Json;

namespace Architect.Configuration;

public class WorldEditorGlobalSettings
{
    public List<string> Favourites = new();

    public Dictionary<string, bool> ContentPackSettings = new();

    public bool CanEnableEditing = false;
    
    [JsonConverter(typeof(PlayerActionSetConverter))]
    public readonly WorldEditorKeyBinds Keybinds = new();

    [JsonConverter(typeof(CustomSceneData.CustomSceneDataConverter))]
    public CustomSceneData Edits = new();
}