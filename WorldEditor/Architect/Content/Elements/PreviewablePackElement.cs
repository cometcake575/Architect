using Architect.Objects;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Content.Elements;

public class PreviewablePackElement(
    GameObject obj,
    string name,
    string category,
    [CanBeNull] Sprite sprite = null,
    int weight = 0)
    : SimplePackElement(obj, name, category, sprite, weight)
{
    public override void PostPlace(ObjectPlacement placement, GameObject preview)
    {
        var obj = placement.SpawnObject();
        obj.transform.parent = preview.transform;
    }
}