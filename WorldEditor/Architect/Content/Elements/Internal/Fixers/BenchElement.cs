using System.Collections.Generic;
using Architect.Content.Groups;
using Architect.Objects;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class BenchElement : GInternalPackElement
{
    public BenchElement(string scene, string path, string name, string category, int weight, float offset = 0) : base(scene, path, name, category, weight, offset)
    {
        WithRotationGroup(RotationGroup.All);
        WithConfigGroup(ConfigGroup.Benches);
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);
        GameObject.AddComponent<BenchConfig>();
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var bench = gameObject.GetComponentInChildren<RestBench>().gameObject;
        var fsm = bench.LocateMyFSM("Bench Control");
        
        fsm.FsmVariables.FindFsmBool("Set Respawn").Value = gameObject.GetComponent<BenchConfig>().setSpawn;
        
        if (rotation == 0) return;
        fsm.FsmVariables
            .FindFsmBool("Tilter").Value = true;
        bench.AddComponent<RestBenchTilt>().tilt = rotation;
    }

    public override void PostPlace(ObjectPlacement placement, GameObject preview)
    {
        var obj = placement.SpawnObject();
        obj.transform.parent = preview.transform;
        var ls = obj.transform.lossyScale;
        
        ls.x /= Mathf.Max(0.01f, preview.transform.localScale.x);
        ls.y /= Mathf.Max(0.01f, preview.transform.localScale.y);
        ls.z /= Mathf.Max(0.01f, preview.transform.localScale.z);

        obj.transform.localScale = ls;
        foreach (var comp in obj.GetComponentsInChildren<Renderer>()) comp.enabled = false;
    }
}

internal class BenchConfig : MonoBehaviour
{
    public bool setSpawn = true;
}