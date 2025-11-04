using System;
using System.Collections.Generic;
using Architect.Attributes;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class LeverPackElement : GInternalPackElement
{
    public LeverPackElement(string scene, string path, string name, int weight = 0) : base(scene, path, name,
        "Interactable", weight)
    {
        WithBroadcasterGroup(BroadcasterGroup.PersistentLevers);
        WithConfigGroup(ConfigGroup.Levers);
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);

        var scale = GameObject.transform.localScale;
        scale.y = Mathf.Abs(scale.y);
        GameObject.transform.localScale = scale;
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Switch Control");

        var str = fsm.FsmVariables.FindFsmString("Player Data");
        if (str != null) str.Value = "";

        try
        {
            fsm.RemoveTransition("Activated", "FINISHED");
        }
        catch (OverflowException)
        {
        }

        fsm.AddCustomAction("Activated", makerFsm =>
        {
            EventManager.BroadcastEvent(makerFsm.gameObject, "OnPull");
            EventManager.BroadcastEvent(makerFsm.gameObject, "LoadedPulled");
        });
        fsm.AddCustomAction("Open", makerFsm =>
        {
            EventManager.BroadcastEvent(makerFsm.gameObject, "OnPull");
            EventManager.BroadcastEvent(makerFsm.gameObject, "FirstPull");
        });

        if (scale > 1) return;
        var rc = gameObject.transform.Find("Range Check");
        if (rc) rc.transform.localScale /= scale;
    }
}