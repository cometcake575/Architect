using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class TollBenchElement : InternalPackElement
{
    private GameObject _gameObject;

    public TollBenchElement(int weight) : base("Toll Bench", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.TollBench);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Fungus3_50", "Toll Machine Bench"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Fungus3_50"]["Toll Machine Bench"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation, float scale)
    {
        gameObject.transform.GetChild(2).name = gameObject.name + " Bench";
    }
}