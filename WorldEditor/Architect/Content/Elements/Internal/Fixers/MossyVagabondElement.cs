using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class MossyVagabondElement : InternalPackElement
{
    private GameObject _gameObject;

    public MossyVagabondElement() : base("Mossy Vagabond", "Enemies")
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Fungus3_39", "Moss Knight Fat"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Fungus3_39"]["Moss Knight Fat"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotatio, float scale)
    {
        gameObject.LocateMyFSM("FSM").SetFsmTemplate(null);
    }
}