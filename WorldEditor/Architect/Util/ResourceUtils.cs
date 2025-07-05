using System;
using System.Reflection;
using SFCore.Utils;
using UnityEngine;

namespace Architect.Util;

public static class ResourceUtils
{
    internal static Sprite Load(string spritePath, FilterMode filterMode = FilterMode.Bilinear, Type modType = null, float ppu = 100)
    {
        return Load(spritePath, new Vector2(0.5f, 0.5f), filterMode, modType, ppu);
    }

    internal static Sprite Load(string spritePath, Vector2 pivot, FilterMode filterMode = FilterMode.Bilinear, Type modType = null, float ppu = 100)
    {
        Assembly asm;
        string path;

        if (modType != null)
        {
            asm = modType.Assembly;
            path = spritePath;
        } else
        {
            asm = Assembly.GetExecutingAssembly();
            path = $"Architect.Resources.{spritePath}.png";
        }
        
        using var s = asm.GetManifestResourceStream(path);
        if (s == null) return null;
        var buffer = new byte[s.Length];
        _ = s.Read(buffer, 0, buffer.Length);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(buffer, true);
        
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, ppu);
        sprite.texture.filterMode = filterMode;
        return sprite;
    }

    internal static AudioClip LoadClip(string clipName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var s = asm.GetManifestResourceStream($"Architect.Resources.{clipName}.wav");

        if (s == null) return null;

        var buffer = new byte[s.Length];
        _ = s.Read(buffer, 0, buffer.Length);

        return WavUtils.ToAudioClip(buffer);
    }

    public static Vector3 FixOffset(Vector3 offset, bool flipped, float rotation, float scale)
    {
        if (flipped) offset.x = -offset.x;
        return Quaternion.Euler(0, 0, rotation) * (offset * scale);
    }

    public static Sprite ConvertFrom2DToolkit(tk2dSpriteDefinition def, float ppu)
    {
        if (def.material.mainTexture is not Texture2D texture) return null;
        var minX = def.uvs[0].x;
        var minY = def.uvs[0].y;
        var maxX = def.uvs[0].x;
        var maxY = def.uvs[0].y;
        for (var i = 1; i < def.uvs.Length; i++)
        {
            minX = Mathf.Min(minX, def.uvs[i].x);
            minY = Mathf.Min(minY, def.uvs[i].y);
            maxX = Mathf.Max(maxX, def.uvs[i].x);
            maxY = Mathf.Max(maxY, def.uvs[i].y);
        }
        
        var x = minX * texture.width;
        var y = minY * texture.height;
        var width = (maxX - minX) * texture.width;
        var height = (maxY - minY) * texture.height;
        
        return Sprite.Create(texture, new Rect(x, y, width, height),
            new Vector2(0.5f, 0.5f), ppu);
    }
}