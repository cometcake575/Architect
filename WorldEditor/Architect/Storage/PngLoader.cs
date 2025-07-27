using System.Collections.Generic;
using System.IO;
using Architect.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Storage;

public static class PngLoader
{
    public static readonly Dictionary<string, Sprite> Sprites = new();

    private static readonly Vector2 Pivot = new(0.5f, 0.5f);
    
    public static void RefreshSprites()
    {
        foreach (var sp in Sprites.Values) Object.Destroy(sp);
        
        foreach (var file in Directory.GetFiles(SceneSaveLoader.DataPath + "Architect/"))
        {
            if (!file.EndsWith(".png")) continue;
            var sprite = ResourceUtils.Load(file, Pivot);
            Sprites[Path.GetFileNameWithoutExtension(file)] = sprite;
        }
    }

    [CanBeNull]
    public static Sprite TryGetSprite(string name)
    {
        if (!Sprites.ContainsKey(name))
        {
            Sprites[name] = ResourceUtils.Load(SceneSaveLoader.DataPath + "Architect/" + name + ".png", Pivot);
        }

        return Sprites[name];
    }
}