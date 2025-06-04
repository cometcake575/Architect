using System.IO;
using System.Reflection;
using UnityEngine;

namespace Architect.utils;

public static class WESpriteUtils
{
    internal static Sprite Load(string spriteName) {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream s = asm.GetManifestResourceStream($"Architect.{spriteName}.png");
        if (s == null) return null;
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(buffer, true);
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public static Sprite ConvertFrom2DToolkit(tk2dSpriteDefinition def)
    {
        Texture2D texture = def.material.mainTexture as Texture2D;
        
        if (!texture) return null;
        
        float minX = def.uvs[0].x;
        float minY = def.uvs[0].y;
        float maxX = def.uvs[0].x;
        float maxY = def.uvs[0].y;
        for (int i = 1; i < def.uvs.Length; i++)
        {
            minX = Mathf.Min(minX, def.uvs[i].x);
            minY = Mathf.Min(minY, def.uvs[i].y);
            maxX = Mathf.Max(maxX, def.uvs[i].x);
            maxY = Mathf.Max(maxY, def.uvs[i].y);
        }
        
        float x = minX * texture.width;
        float y = minY * texture.height;
        float width = (maxX - minX) * texture.width;
        float height = (maxY - minY) * texture.height;
        
        return Sprite.Create(texture, new Rect(x, y, width, height),
            new Vector2(0.5f, 0.5f), 64);
    }
}