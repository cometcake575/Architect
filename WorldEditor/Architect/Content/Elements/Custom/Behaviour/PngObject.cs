using System;
using Architect.Storage;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class PngObject : MonoBehaviour
{
    public string url;
    public bool point;
    public float ppu = 100;
    
    private void Awake()
    {
        if (string.IsNullOrEmpty(url)) return;
        CustomAssetLoader.DoLoadSprite(gameObject, url, point, ppu);
    }
}