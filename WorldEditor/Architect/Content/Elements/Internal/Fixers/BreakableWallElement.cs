using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class BreakableWallElement : GInternalPackElement
{
    public BreakableWallElement(string scene, string path, string name, int weight) : base(scene, path, name, "Interactable", weight)
    {
        WithRotationGroup(RotationGroup.Four);
        WithConfigGroup(ConfigGroup.Breakable);
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        base.AfterPreload(preloads);
        
        for (var i = 0; i < GameObject.transform.childCount; i++)
        {
            var obj = GameObject.transform.GetChild(i).gameObject;
            if (obj.name == "Masks") obj.SetActive(false);
        }
    }
}
