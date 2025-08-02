using System.Collections;
using System.Collections.Generic;
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

    public static void WipeAllImages()
    {
        foreach (var sp in Sprites.Values) Object.Destroy(sp);
        Sprites.Clear();
    }

    public static void DoLoadSprite(GameObject obj, string url, bool point, float ppu)
    {
        GameManager.instance.StartCoroutine(LoadSprite(url, point, ppu, obj));
    }

    private static IEnumerator LoadSprite(string url, bool point, float ppu, [CanBeNull] GameObject obj = null)
    {
        var id = url + (point ? "_point_" : "_bilinear_") + ppu;
        if (!Sprites.ContainsKey(id))
        {
            var path = GetPath(url);
            var tmp = ResourceUtils.Load(path, Pivot, point, ppu);
            if (!tmp)
            {
                var task = Task.Run(() => SaveImage(url, path));
                while (!task.IsCompleted) yield return null;
                tmp = ResourceUtils.Load(path, Pivot, point, ppu);
            }
            Sprites[id] = tmp;
        }

        if (obj) obj.GetComponent<SpriteRenderer>().sprite = Sprites[id];
        yield return null;
    }
    
    public static void PrepareImage(string url, bool point, float ppu)
    {
        GameManager.instance.StartCoroutine(LoadSprite(url, point, ppu));
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