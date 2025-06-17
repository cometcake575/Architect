using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal;

internal class GInternalPackElement : InternalPackElement
{
    private readonly string _scene;
    private readonly string _path;
    
    private readonly float _offset;

    protected GameObject GameObject;

    public GInternalPackElement(string scene, string path, string name, string category, int weight, float offset = 0) : base(name, category, weight)
    {
        _scene = scene;
        _path = path;
        _offset = offset;
    }
    
    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add((_scene, _path));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        GameObject = preloads[_scene][_path];
        GameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        var z = GameObject.transform.position.z + _offset;
        var setZ = GameObject.GetComponent<SetZ>();
        if (setZ && setZ.enabled) z = setZ.z;
        GameObject.transform.SetPositionZ(z);
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return GameObject;
    }
}