using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Architect.Storage;

namespace Architect;

public static class ExternalLoader
{
    private static readonly Dictionary<string, List<string>> StandaloneMaps = [];
    
    private static void LoadStandaloneMap(string sceneName, string mapPath)
    {
        if (!StandaloneMaps.ContainsKey(sceneName)) StandaloneMaps[sceneName] = [];
        StandaloneMaps[sceneName].Add(mapPath);
    }
 
    public static LevelData GetDataForScene(string sceneName)
    {
        var ld = new LevelData([], []);
        if (StandaloneMaps.TryGetValue(sceneName, out var scenes))
        {
            foreach (var data in scenes.Select(path => SceneSaveLoader.DeserializeSceneData(File.ReadAllText(path))))
            {
                ld.Placements.AddRange(data.Placements);
                foreach (var tc in data.TileChanges.Where(tc => !ld.TileChanges.Remove(tc))) 
                    ld.TileChanges.Add(tc);
            }
        }

        return ld;
    }
    
    public static void FindMaps()
    {
        var modsPath = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
        while (!modsPath.EndsWith("Mods"))
        {
            modsPath = Directory.GetParent(modsPath)?.FullName;
            if (modsPath == null) return;
        }
        foreach (var dir in Directory.GetDirectories(
                     modsPath, 
                     "ArchitectLegacy",
                     SearchOption.AllDirectories))
        {
            if (dir == Path.Combine(modsPath, "ArchitectLegacy")) continue;

            foreach (var path in Directory.GetFiles(dir))
            {
                if (!path.EndsWith(".architect.json")) continue;
                var sceneName = Path.GetFileNameWithoutExtension(path).Replace(".architect", "");
                LoadStandaloneMap(sceneName, path);
            }
            
            CustomAssetLoader.AssetPaths.Add(dir);
        }
    }
}