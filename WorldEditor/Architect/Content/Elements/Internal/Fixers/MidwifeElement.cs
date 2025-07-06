using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class MidwifeElement : InternalPackElement
{
    private GameObject _gameObject;

    public MidwifeElement(int weight) : base("Midwife", "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.Four);
        WithConfigGroup(ConfigGroup.Midwife);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Deepnest_41", "Happy Spider NPC"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Deepnest_41"]["Happy Spider NPC"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        if (flipped == Mathf.Approximately(rotation, 180)) return;
        gameObject.transform.GetChild(1).gameObject.LocateMyFSM("npc_control").FsmVariables
            .FindFsmFloat("Move To Offset")
            .Value = -3;
    }
}