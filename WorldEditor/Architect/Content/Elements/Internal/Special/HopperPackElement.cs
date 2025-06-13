using System.Collections.Generic;
using Architect.Content.Groups;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Special;

internal class HopperPackElement : GInternalPackElement
{
    public HopperPackElement(string scene, string path, string name) : base(scene, path, name, "Enemies", 0, 0)
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation)
    {
        var fsm = gameObject.LocateMyFSM("Hopper");

        if (GetName() == "Hopper")
        {
            var scale = gameObject.transform.localScale;
            scale.x = -scale.x;
            gameObject.transform.localScale = scale;
            if (flipped) return;
        } else if (!flipped) return;
        
        var b = fsm.FsmVariables.FindFsmBool("Moving Right");
        b.Value = !b.Value;
    }
}