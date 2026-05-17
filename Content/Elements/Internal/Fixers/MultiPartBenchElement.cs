using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class MultiPartBenchElement : BenchElement
{
    private readonly string _extra;
    private readonly string _scene;

    public MultiPartBenchElement(string scene, string path, string extra, string name, string category, int weight = 0)
        : base(scene, path, name, category, weight)
    {
        _scene = scene;
        _extra = extra;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        base.AddPreloads(preloadInfo);
        preloadInfo.Add((_scene, _extra));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);
        var extraPart = preloads[_scene][_extra];
        extraPart.transform.parent = GameObject.transform;
        extraPart.SetActive(true);
    }
}