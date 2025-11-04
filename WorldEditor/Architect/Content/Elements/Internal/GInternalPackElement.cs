using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Content.Elements.Internal;

internal class GInternalPackElement : InternalPackElement
{
    private readonly float _offset;
    private readonly string _path;
    private readonly string _scene;

    [CanBeNull] private Sprite _sprite;

    protected GameObject GameObject;

    public GInternalPackElement(string scene, string path, string name, string category, int weight, float offset = 0) :
        base(name, category, weight)
    {
        _scene = scene;
        _path = path;
        _offset = offset;
    }

    internal GInternalPackElement WithSprite(Sprite sprite)
    {
        _sprite = sprite;
        return this;
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

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return GameObject;
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }
}