using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Architect.Attributes.Config;
using Architect.Objects;
using Architect.UI;
using Modding.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace Architect.Storage;

public static class SceneSaveLoader
{
    internal static readonly ObjectPlacement.ObjectPlacementConverter Opc = new();
    internal static readonly Vector3Converter V3C = new();

    public static string DataPath;

    private static readonly Dictionary<string, List<string>> ScheduledErases = new();
    private static readonly Dictionary<string, List<(int, int)>> ScheduledTileChanges = new();
    private static readonly Dictionary<string, List<(string, Vector3)>> ScheduledUpdates = new();
    private static readonly Dictionary<string, List<ObjectPlacement>> ScheduledEdits = new();

    internal static void Initialize()
    {
        DataPath = Path.GetFullPath(Application.persistentDataPath + "/");
        Directory.CreateDirectory(DataPath + "Architect/");

        On.GameManager.SaveGame += (orig, self) =>
        {
            List<string> scenes = [];

            scenes.AddRange(ScheduledErases.Keys);
            scenes.AddRange(ScheduledTileChanges.Keys);
            scenes.AddRange(ScheduledEdits.Keys);
            scenes.AddRange(ScheduledUpdates.Keys);

            foreach (var scene in scenes)
            {
                var levelData = LoadScene(scene);
                if (ScheduledErases.TryGetValue(scene, out var erases))
                    levelData.Placements.RemoveAll(obj => erases.Contains(obj.GetId()));
                if (ScheduledEdits.TryGetValue(scene, out var edits)) levelData.Placements.AddRange(edits);
                if (ScheduledTileChanges.TryGetValue(scene, out var tileChanges))
                {
                    foreach (var tile in tileChanges) levelData.ToggleTile(tile);
                }
                if (ScheduledUpdates.TryGetValue(scene, out var updates))
                    foreach (var pair in updates)
                        levelData.Placements.First(obj => obj.GetId() == pair.Item1).Move(pair.Item2);
                SaveScene(scene, levelData);
            }

            ScheduledTileChanges.Clear();
            ScheduledErases.Clear();
            ScheduledEdits.Clear();
            ScheduledUpdates.Clear();

            orig(self);
        };
    }

    public static LevelData LoadScene(string name)
    {
        return Load("Architect/" + name);
    }

    public static LevelData Load(string name)
    {
        var path = DataPath + name + ".architect.json";
        if (!File.Exists(path)) return new LevelData([], []);

        var content = File.ReadAllText(path);
        var data = DeserializeSceneData(content);

        if (ScheduledErases.TryGetValue(name, out var erase))
        {
            data.Placements.RemoveAll(obj => erase.Contains(obj.GetId()));
            ScheduledErases.Remove(name);
        }

        if (ScheduledEdits.TryGetValue(name, out var edit))
        {
            data.Placements.AddRange(edit);
            ScheduledEdits.Remove(name);
        }

        if (ScheduledTileChanges.TryGetValue(name, out var tileChanges))
        {
            foreach (var tile in tileChanges) data.ToggleTile(tile);
            ScheduledTileChanges.Remove(name);
        }

        if (ScheduledUpdates.TryGetValue(name, out var scheduledUpdate))
        {
            foreach (var update in scheduledUpdate) data.Placements.First(obj => obj.GetId() == update.Item1).Move(update.Item2);

            ScheduledUpdates.Remove(name);
        }

        return data;
    }

    public static void SaveScene(string name, LevelData placements)
    {
        Save("Architect/" + name, placements);
    }

    public static void Save(string name, LevelData placements)
    {
        var data = SerializeSceneData(placements);
        Save(name, data);
    }

    public static void Save(string name, string data)
    {
        var path = DataPath + name + ".architect.json";
        if (File.Exists(path)) File.Delete(path);

        if (data == "[]") return;

        using var stream = File.Create(path);
        using var writer = new StreamWriter(stream);

        writer.Write(data);
    }

    public static void WipeScene(string name)
    {
        var path = DataPath + "Architect/" + name + ".architect.json";
        if (File.Exists(path)) File.Delete(path);
    }

    public static void WipeAllScenes()
    {
        CustomAssetLoader.WipeAssets();
        foreach (var file in Directory.GetFiles(DataPath + "Architect/")) File.Delete(file);
    }

    public static LevelData DeserializeSceneData(string data)
    {
        return JsonConvert.DeserializeObject<LevelData>(data);
    }

    public static string SerializeSceneData(LevelData placements)
    {
        return JsonConvert.SerializeObject(placements,
            Opc,
            V3C
        );
    }

    public static string SerializeAllScenes()
    {
        Dictionary<string, string> data = new();
        foreach (var file in Directory.GetFiles(DataPath + "Architect/"))
        {
            var name = Path.GetFileName(file);
            if (!name.EndsWith(".architect.json")) continue;

            data[name.Replace(".architect.json", "")] = File.ReadAllText(file);
        }

        return JsonConvert.SerializeObject(data);
    }

    public static IEnumerator LoadAllScenes(Dictionary<string, string> placements)
    {
        WipeAllScenes();
        var passed = true;

        var startTime = Time.realtimeSinceStartup;

        foreach (var pair in placements)
        {
            var task = Task.Run(() => LoadEdits(pair.Value));
            while (!task.IsCompleted) yield return null;

            if (!task.Result) passed = false;

            Save("Architect/" + pair.Key, pair.Value);
        }

        var elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < 1) yield return new WaitForSeconds(1 - elapsed);

        LevelSharingManager.EnableControls();
        LevelSharingManager.ShowStatus(passed ? "Download Complete" : "Error downloading some assets");
    }

    private static async Task<bool> LoadEdits(string data)
    {
        var passed = true;
        foreach (var config in DeserializeSceneData(data).Placements.Select(obj => obj.Config
                     .ToDictionary(conf => conf.GetName())))
        {
            if (!await TryDownload(config, "Source URL", CustomAssetLoader.GetSpritePath)) passed = false;
            if (!await TryDownload(config, "Clip URL", CustomAssetLoader.GetSoundPath)) passed = false;
            if (!await TryDownload(config, "Video URL", CustomAssetLoader.GetVideoPath)) passed = false;
        }

        return passed;
    }

    private static async Task<bool> TryDownload(Dictionary<string, ConfigValue> config, string type,
        Func<string, string> getPath)
    {
        var passed = true;
        if (config.TryGetValue(type, out var val) && val is StringConfigValue stringVal)
        {
            var url = stringVal.GetValue();
            var path = getPath.Invoke(url);
            if (!await CustomAssetLoader.SaveFile(url, path)) passed = false;
        }

        return passed;
    }

    public static void ScheduleErase(string scene, string id)
    {
        if (!ScheduledErases.ContainsKey(scene)) ScheduledErases[scene] = [];
        ScheduledErases[scene].Add(id);
    }

    public static void ScheduleTileChange(string scene, List<(int, int)> tiles)
    {
        if (!ScheduledTileChanges.ContainsKey(scene)) ScheduledTileChanges[scene] = [];
        ScheduledTileChanges[scene].AddRange(tiles);
    }

    public static void ScheduleEdit(string scene, ObjectPlacement placement)
    {
        if (!ScheduledEdits.ContainsKey(scene)) ScheduledEdits[scene] = [];
        ScheduledEdits[scene].Add(placement);
    }

    public static void ScheduleUpdate(string scene, string id, Vector3 pos)
    {
        if (!ScheduledUpdates.ContainsKey(scene)) ScheduledUpdates[scene] = [];
        ScheduledUpdates[scene].Add((id, pos));
    }
}