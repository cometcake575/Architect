using System.Collections.Generic;
using System.IO;
using System.Linq;
using Architect.Attributes.Config;
using Architect.Objects;
using Modding.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace Architect.Storage;

public static class SceneSaveLoader
{
    internal static readonly ObjectPlacement.ObjectPlacementConverter Opc = new();
    internal static readonly Vector3Converter V3C = new();

    public static string DataPath; 

    internal static void Initialize()
    {
        DataPath = Path.GetFullPath(Application.persistentDataPath + "/");
        Directory.CreateDirectory(DataPath + "Architect/");

        On.GameManager.SaveGame += (orig, self) =>
        {
            List<string> scenes = [];
            
            scenes.AddRange(ScheduledErases.Keys);
            scenes.AddRange(ScheduledEdits.Keys);
            scenes.AddRange(ScheduledUpdates.Keys);

            foreach (var scene in scenes)
            {
                var placements = LoadScene(scene);
                if (ScheduledErases.TryGetValue(scene, out var erases)) placements.RemoveAll(obj => erases.Contains(obj.GetId()));
                if (ScheduledEdits.TryGetValue(scene, out var edits)) placements.AddRange(edits);
                if (ScheduledUpdates.TryGetValue(scene, out var updates))
                    foreach (var pair in updates)
                    {
                        placements.First(obj => obj.GetId() == pair.Item1).Move(pair.Item2);
                    }
                SaveScene(scene, placements);
            }
            
            orig(self);
        };
    }

    public static List<ObjectPlacement> LoadScene(string name)
    {
        return Load("Architect/" + name);
    }
    
    public static List<ObjectPlacement> Load(string name)
    {
        var path = DataPath + name + ".architect.json";
        if (!File.Exists(path)) return [];
        
        var content = File.ReadAllText(path);
        var data = DeserializeSceneData(content);

        if (ScheduledErases.TryGetValue(name, out var erase))
        {
            data.RemoveAll(obj => erase.Contains(obj.GetId()));
            ScheduledErases.Remove(name);
        }

        if (ScheduledEdits.TryGetValue(name, out var edit))
        {
            data.AddRange(edit);
            ScheduledErases.Remove(name);
        }

        if (ScheduledUpdates.TryGetValue(name, out var scheduledUpdate))
        {
            foreach (var update in scheduledUpdate)
            {
                data.First(obj => obj.GetId() == update.Item1).Move(update.Item2);
            }

            ScheduledUpdates.Remove(name);
        }
        
        return data;
    }
    
    public static void SaveScene(string name, List<ObjectPlacement> placements)
    {
        Save("Architect/" + name, placements);
    }
    
    public static void Save(string name, List<ObjectPlacement> placements)
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
        PngLoader.WipeAllImages();
        foreach (var file in Directory.GetFiles(DataPath + "Architect/")) File.Delete(file);
    }

    public static List<ObjectPlacement> DeserializeSceneData(string data)
    {
        return JsonConvert.DeserializeObject<List<ObjectPlacement>>(data);
    }

    public static string SerializeSceneData(List<ObjectPlacement> placements)
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

    public static void LoadAllScenes(Dictionary<string, string> placements)
    {
        WipeAllScenes();
        foreach (var pair in placements)
        {
            foreach (var placement in DeserializeSceneData(pair.Value))
            {
                var source = placement.Config.FirstOrDefault(config => config.GetName() == "Source URL");
                if (source is not StringConfigValue value) continue;
                
                var filter = placement.Config.FirstOrDefault(config => config.GetName() == "Filter");
                var ppu = placement.Config.FirstOrDefault(config => config.GetName() == "Pixels Per Unit");
                
                PngLoader.PrepareImage(
                    value.GetValue(), 
                    filter is ChoiceConfigValue filterValue && filterValue.GetValue() == 0,
                    ppu is FloatConfigValue ppuValue ? ppuValue.GetValue() : 100
                    );
            }

            Save("Architect/" + pair.Key, pair.Value);
        }
    }

    public static void LoadAllScenes(Dictionary<string, List<ObjectPlacement>> placements)
    {
        WipeAllScenes();
        foreach (var pair in placements)
        {
            SaveScene(pair.Key, pair.Value);
        }
    }

    public static void ScheduleErase(string scene, string id)
    {
        if (!ScheduledErases.ContainsKey(scene)) ScheduledErases[scene] = [];
        ScheduledErases[scene].Add(id);
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

    private static readonly Dictionary<string, List<string>> ScheduledErases = new();
    private static readonly Dictionary<string, List<(string, Vector3)>> ScheduledUpdates = new();
    private static readonly Dictionary<string, List<ObjectPlacement>> ScheduledEdits = new();
}