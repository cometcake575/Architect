using System.Collections.Generic;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class MultiPartInternalElement : InternalPackElement
{
    private GameObject _gameObject;
    private readonly string _scene;
    private readonly string _path;
    private readonly string _extra;

    public MultiPartInternalElement(string scene, string path, string extra, string name, string category, int weight = 0) : base(name, category, weight)
    {
        _scene = scene;
        _path = path;
        _extra = extra;
    }

    public override GameObject GetPrefab(bool flipped, int rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add((_scene, _path));
        preloadInfo.Add((_scene, _extra));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads[_scene][_path];
        var extraPart = preloads[_scene][_extra];
        extraPart.transform.parent = _gameObject.transform;
        extraPart.SetActive(true);
    }
}