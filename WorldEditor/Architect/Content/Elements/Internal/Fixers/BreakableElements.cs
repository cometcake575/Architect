using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class BreakableWallElement : GInternalPackElement
{
    public BreakableWallElement(string scene, string path, string name, int weight) : base(scene, path, name,
        "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.Four);
        WithConfigGroup(ConfigGroup.Breakable);
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);

        for (var i = 0; i < GameObject.transform.childCount; i++)
        {
            var obj = GameObject.transform.GetChild(i).gameObject;
            if (obj.name == "Masks") obj.SetActive(false);
        }
    }
}

internal class DiveGroundElement : InternalPackElement
{
    private GameObject _gameObject;

    public DiveGroundElement(int weight) : base("Dive Ground", "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.Four);
        WithConfigGroup(ConfigGroup.Breakable);
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Crossroads_52", "Quake Floor"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_52"]["Quake Floor"];

        var child = _gameObject.transform.GetChild(0).GetChild(0);
        for (var i = 0; i < 4; i++) child.GetChild(i).gameObject.SetActive(false);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }
}

internal class BreakableCoffinElement : InternalPackElement
{
    private GameObject _gameObject;

    public BreakableCoffinElement(int weight) : base("Dive Coffin", "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.Four);
        WithConfigGroup(ConfigGroup.Breakable);
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("RestingGrounds_05", "Quake Floor"));
        preloadInfo.Add(("RestingGrounds_05", "rg_coffin_break_front"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["RestingGrounds_05"]["rg_coffin_break_front"];
        var child = preloads["RestingGrounds_05"]["Quake Floor"];
        child.transform.parent = _gameObject.transform;
        child.SetActive(true);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        gameObject.transform.GetChild(2).name = gameObject.name + " Quake Floor";
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }
}