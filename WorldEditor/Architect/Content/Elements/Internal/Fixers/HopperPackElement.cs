using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class HopperPackElement : GInternalPackElement
{
    public HopperPackElement(string scene, string path, string name) : base(scene, path, name, "Enemies", 0)
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Enemies);
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, int rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM("Hopper");

        if (GetName() == "Hopper")
        {
            var oldScale = gameObject.transform.localScale;
            oldScale.x = -oldScale.x;
            gameObject.transform.localScale = oldScale;
            if (flipped) return;
        } else if (!flipped) return;
        
        var b = fsm.FsmVariables.FindFsmBool("Moving Right");
        b.Value = !b.Value;
    }
}