using System.Collections.Generic;
using Architect.Attributes;
using Architect.Attributes.Broadcasters;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Special;

internal class LeverPackElement : GInternalPackElement
{
    public LeverPackElement(string scene, string path, string name) : base(scene, path, name, "Interactable", 0, 20)
    {
        WithBroadcasterGroup(BroadcasterGroup.Levers);
        WithConfigGroup(ConfigGroup.Levers);
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);
        
        var scale = GameObject.transform.localScale;
        scale.y = Mathf.Abs(scale.y);
        GameObject.transform.localScale = scale;
    }

    public override void PostSpawn(GameObject gameObject)
    {
        var fsm = gameObject.LocateMyFSM("Switch Control");
        fsm.AddCustomAction("Activated", makerFsm =>
        {
            EventManager.BroadcastEvent(makerFsm.gameObject, EventBroadcasterType.LoadedPulled);
        });
        fsm.AddCustomAction("Open", makerFsm =>
        {
            EventManager.BroadcastEvent(makerFsm.gameObject, EventBroadcasterType.OnPull);
        });
    }
}