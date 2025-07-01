using System.Collections.Generic;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class MillibellePackElement : InternalPackElement
{
    private GameObject _gameObject;

    public MillibellePackElement(int weight) : base("Millibelle", "Interactable", weight)
    {
        FlipVertical();
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Ruins_Bathhouse", "Banker Spa NPC"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Ruins_Bathhouse"]["Banker Spa NPC"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Hit Around");
        fsm.DisableAction("Init", 1);
        fsm.DisableAction("Init", 2);
        
        for (var i = 1; i < 4; i++) gameObject.transform.GetChild(i).gameObject.SetActive(false);
    }
}