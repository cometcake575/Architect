using System.Collections.Generic;
using Architect.Content.Groups;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class ShadeGateElement : InternalPackElement
{
    private GameObject _gameObject;
    private readonly Sprite _sprite = ResourceUtils.LoadInternal("shade_gate", new Vector2(0.6600646809f, 0.2272230928f), ppu:64);

    public ShadeGateElement(int weight) : base("Shade Gate", "Interactable", weight:weight)
    {
        WithRotationGroup(RotationGroup.Eight);
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _gameObject;
    }

    internal override void AddPreloads(List<(string, string)> preloadInfo)
    {
        preloadInfo.Add(("Fungus3_44", "shadow_gate"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Fungus3_44"]["shadow_gate"];
        _gameObject.transform.GetChild(1).gameObject.SetActive(false);
        _gameObject.transform.GetChild(5).gameObject.SetActive(false);
    }
}