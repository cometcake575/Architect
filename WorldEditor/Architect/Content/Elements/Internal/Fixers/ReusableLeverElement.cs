using System.Collections.Generic;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ReusableLeverElement : InternalPackElement
{
    private GameObject _gameObject;

    public ReusableLeverElement(int weight) : base("Reusable Lever", "Interactable", weight)
    {
        WithBroadcasterGroup(BroadcasterGroup.Levers);
        WithRotationGroup(RotationGroup.Eight);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Ruins1_03", "Lift Call Lever"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Ruins1_03"]["Lift Call Lever"];
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Call Lever");
        
        fsm.DisableAction("Send Msg", 2);
        fsm.InsertCustomAction("Check Already Called", makerFsm =>
        {
            EventManager.BroadcastEvent(makerFsm.gameObject, "OnPull");
        }, 0);
    }
}