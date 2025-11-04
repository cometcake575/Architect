using System.Collections.Generic;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ForceActivatedElement(
    string scene,
    string path,
    string name,
    string category,
    int weight = 0,
    float offset = 0)
    : GInternalPackElement(scene, path, name, category, weight, offset)
{
    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);

        GameObject.RemoveComponent<DeactivateIfPlayerdataFalse>();
        GameObject.RemoveComponent<DeactivateIfPlayerdataTrue>();
    }
}