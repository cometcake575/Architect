using System.Collections.Generic;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class AcidElement() : InternalPackElement("Acid", "Hazards")
{
    private GameObject _gameObject;

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Fungus3_26", "Acid Control v2"));
        preloadInfo.Add(("Fungus3_26", "water_fog"));
        preloadInfo.Add(("Fungus3_26", "water_fog (1)"));
        preloadInfo.Add(("Fungus3_26", "water_components"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = new GameObject("Acid");
        Object.DontDestroyOnLoad(_gameObject);
        _gameObject.SetActive(false);
        
        var p0 = preloads["Fungus3_26"]["Acid Control v2"];
        p0.SetActive(true);
        p0.transform.parent = _gameObject.transform;
        
        var p1 = preloads["Fungus3_26"]["water_fog"];
        p1.SetActive(true);
        p1.transform.parent = p0.transform;
        
        var p2 = preloads["Fungus3_26"]["water_fog (1)"];
        p2.SetActive(true);
        p2.transform.parent = p0.transform;
        
        var p3 = preloads["Fungus3_26"]["water_components"];
        p3.SetActive(true);
        p3.transform.parent = p0.transform;
        
        p0.transform.Translate(-12, -5, 0);
    }

    private readonly Sprite _sprite = ResourceUtils.Load("acid");

    public override Sprite GetSprite()
    {
        return _sprite;
    }
}