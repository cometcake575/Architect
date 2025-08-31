using System.Collections.Generic;
using Modding.Converters;
using Newtonsoft.Json;

namespace Architect.Storage;

public class WorldEditorGlobalSettings
{
    public string ApiKey = "";

    public bool CanEnableEditing = false;

    public bool CollaborationMode = false;

    public Dictionary<string, bool> ContentPackSettings = new();
    public List<string> Favourites = [];

    [JsonConverter(typeof(PlayerActionSetConverter))]
    public WorldEditorKeyBinds Keybinds = new();

    public bool TestMode = false;
}