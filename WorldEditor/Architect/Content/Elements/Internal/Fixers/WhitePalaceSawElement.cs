using System.Collections.Generic;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class WhitePalaceSawElement : InternalPackElement
{
    private GameObject _gameObject;

    public WhitePalaceSawElement() : base("White Palace Saw", "Hazards")
    {
        WithConfigGroup(ConfigGroup.MovingObjects);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("White_Palace_07", "wp_saw"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["White_Palace_07"]["wp_saw"];
        _gameObject.layer = LayerMask.NameToLayer("Enemies");
    }
}