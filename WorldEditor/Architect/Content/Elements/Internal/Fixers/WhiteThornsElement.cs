using System.Collections.Generic;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Groups;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class WhiteThornsElement : InternalPackElement
{
    private GameObject _gameObject;

    public WhiteThornsElement() : base("White Thorns", "Hazards")
    {
        WithConfigGroup(ConfigGroup.Thorns);
        WithRotationGroup(RotationGroup.All);
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("White_Palace_07", "white_thorn_0001_1 (78)"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["White_Palace_07"]["white_thorn_0001_1 (78)"];

        _gameObject.transform.SetRotation2D(0);
        _gameObject.transform.localScale = Vector3.one;
        _gameObject.transform.position = Vector3.zero;

        var col = _gameObject.AddComponent<EdgeCollider2D>();

        col.isTrigger = true;
        
        col.points =
        [
            new Vector2(-3.672f, -1.265f),
            new Vector2(-3.011f, 0.066f),
            new Vector2(-1.469f, 0.777f),
            new Vector2(0.474f, 1.282f),
            new Vector2(2.353f, 0.813f),
            new Vector2(3.754f, -0.674f)
        ];
        
        _gameObject.AddComponent<CustomDamager>().damageAmount = 1;
    }
}