using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class GrubfatherElement : InternalPackElement
{
    private GameObject _gameObject;

    public GrubfatherElement(int weight) : base("Grubfather", "Interactable", weight)
    {
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Crossroads_38", "Fat Grub King"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_38"]["Fat Grub King"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation, float scale)
    {
        gameObject.LocateMyFSM("FSM").enabled = false;
    }
}