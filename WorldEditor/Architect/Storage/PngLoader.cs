using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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

    public static void WipeAllImages()
    {
        Sprites.Clear();
    }

    public static void DoLoadSprite(GameObject obj, string url)
    {
        GameManager.instance.StartCoroutine(LoadSprite(url, obj));
    }

    private static IEnumerator LoadSprite(string url, [CanBeNull] GameObject obj = null)
    {
        if (!Sprites.ContainsKey(url))
        {
            var path = GetPath(url);
            
            var tmp = ResourceUtils.Load(path, Pivot);
            if (!tmp)
            {
                var task = Task.Run(() => SaveImage(url, path));
                while (!task.IsCompleted) yield return null;
                tmp = ResourceUtils.Load(path, Pivot);
            }
            Sprites[url] = tmp;
        }

        if (obj) obj.GetComponent<SpriteRenderer>().sprite = Sprites[url];
        yield return null;
    }
    
    public static void PrepareImage(string url)
    {
        GameManager.instance.StartCoroutine(LoadSprite(url));
    }

    private static async Task SaveImage(string url, string path)
    {
        var webClient = new WebClient();
        await webClient.DownloadFileTaskAsync(url, path);
    }

    private static string GetPath(string url)
    {
        var pathUrl = Path.GetInvalidFileNameChars()
            .Aggregate(url, (current, c) => current.Replace(c, '_'));
        return SceneSaveLoader.DataPath + "Architect/" + pathUrl + ".png";
    }
}