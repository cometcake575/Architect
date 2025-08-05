using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Util;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Architect.Storage;

public static class CustomAssetLoader
{
    public static readonly Dictionary<string, Sprite> Sprites = new();
    public static readonly Dictionary<string, AudioClip> Sounds = new();
    
    public static readonly HashSet<string> LoadingSounds = [];

    private static readonly Vector2 Pivot = new(0.5f, 0.5f);

    public static async Task SaveFile(string url, string path)
    {
        var webClient = new WebClient();
        await webClient.DownloadFileTaskAsync(url, path);
    }

    public static void WipeAssets()
    {
        foreach (var sp in Sprites.Values) Object.Destroy(sp);
        foreach (var sp in Sounds.Values) Object.Destroy(sp);
        Sprites.Clear();
        Sounds.Clear();
    }

    public static void DoLoadVideo(GameObject obj, float? scale, string url)
    {
        GameManager.instance.StartCoroutine(LoadVideo(url, scale, obj));
    }

    private static IEnumerator LoadVideo(string url, float? scale, [CanBeNull] GameObject obj = null)
    {
        var path = GetVideoPath(url);
        if (!File.Exists(path))
        {
            var task = Task.Run(() => SaveFile(url, path));
            while (!task.IsCompleted) yield return null;
        }

        if (obj)
        {
            var player = obj.GetComponent<VideoPlayer>();
            player.url = path;
            while (player.width == 0) yield return null;

            var sc = scale.GetValueOrDefault(EditorManager.Scale);
            obj.transform.SetScaleX(sc * player.width / 100);
            obj.transform.SetScaleY(sc * player.height / 100);
        }
    }
    
    public static void PrepareVideo(string url)
    {
        GameManager.instance.StartCoroutine(LoadVideo(url, null));
    }

    public static string GetVideoPath(string url)
    {
        var pathUrl = Path.GetInvalidFileNameChars()
            .Aggregate(url, (current, c) => current.Replace(c, '_'));
        return SceneSaveLoader.DataPath + "Architect/" + pathUrl + ".mov";
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
            var path = GetSpritePath(url);
            var tmp = ResourceUtils.Load(path, Pivot, point, ppu);
            if (!tmp)
            {
                var task = Task.Run(() => SaveFile(url, path));
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

    public static string GetSpritePath(string url)
    {
        var pathUrl = Path.GetInvalidFileNameChars()
            .Aggregate(url, (current, c) => current.Replace(c, '_'));
        return SceneSaveLoader.DataPath + "Architect/" + pathUrl + ".png";
    }

    public static void DoLoadSound(GameObject obj, string url)
    {
        GameManager.instance.StartCoroutine(LoadSound(url, obj));
    }
    
    private static IEnumerator LoadSound(string url, [CanBeNull] GameObject obj = null)
    {
        while (LoadingSounds.Contains(url)) yield break;
        if (!Sounds.ContainsKey(url))
        {
            LoadingSounds.Add(url);
            var path = GetSoundPath(url);
            var tmp = ResourceUtils.LoadClip(path);
            if (!tmp || tmp.length == 0)
            {
                var task = Task.Run(() => SaveFile(url, path));
                while (!task.IsCompleted) yield return null;
                tmp = ResourceUtils.LoadClip(path);
            }
            Sounds[url] = tmp;
            LoadingSounds.Remove(url);
        }

        if (obj) obj.GetComponent<WavObject>().sound = Sounds[url];
        yield return null;
    }
    
    public static void PrepareClip(string url)
    {
        GameManager.instance.StartCoroutine(LoadSound(url));
    }

    public static string GetSoundPath(string url)
    {
        var pathUrl = Path.GetInvalidFileNameChars()
            .Aggregate(url, (current, c) => current.Replace(c, '_'));
        return SceneSaveLoader.DataPath + "Architect/" + pathUrl + ".wav";
    }
}