using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content.Elements.Internal;

internal class GInternalPackElement : InternalPackElement
{
    private readonly string _scene;
    private readonly string _path;

    protected GameObject GameObject;

    public GInternalPackElement(string scene, string path, string name, string category, int weight) : base(name, category, weight)
    {
        _scene = scene;
        _path = path;
    }
    
    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add((_scene, _path));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        GameObject = preloads[_scene][_path];
        GameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        var setZ = GameObject.GetComponent<SetZ>();
        if (!setZ) return;
        GameObject.transform.SetPositionZ(setZ.z);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return GameObject;
    }
}