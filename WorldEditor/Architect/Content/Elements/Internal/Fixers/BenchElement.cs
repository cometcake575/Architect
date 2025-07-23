using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class BenchElement : GInternalPackElement
{
    public BenchElement(string scene, string path, string name, string category, int weight, float offset = 0) : base(scene, path, name, category, weight, offset)
    {
        WithRotationGroup(RotationGroup.All);
    }
    
    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        if (rotation == 0) return;
        var bench = gameObject.GetComponentInChildren<RestBench>().gameObject;
        bench.LocateMyFSM("Bench Control").FsmVariables
            .FindFsmBool("Tilter").Value = true;
        bench.AddComponent<RestBenchTilt>().tilt = rotation;
    }
}