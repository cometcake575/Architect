using System.Collections.Generic;
using Architect.Objects;
using Modding.Converters;
using Newtonsoft.Json;

namespace Architect.Configuration;

public class WorldEditorGlobalSettings
{
    public List<string> Favourites = new();

    public string ApiKey = "";

    public Dictionary<string, bool> ContentPackSettings = new();

    public bool CanEnableEditing = false;
    
    [JsonConverter(typeof(PlayerActionSetConverter))]
    public readonly WorldEditorKeyBinds Keybinds = new();

    public Dictionary<string, List<ObjectPlacement>> Edits = new();
}