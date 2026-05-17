using System.Collections.Generic;
using Architect.Content.Groups;
using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class CharmElement : InternalPackElement
{
    private readonly Sprite _sprite = ResourceUtils.LoadInternal("shiny", new Vector2(0.5f, 0.5f), ppu: 64);
    private GameObject _gameObject;

    public CharmElement(int weight) : base("Charm", "Interactable", weight)
    {
        WithConfigGroup(ConfigGroup.Charm);
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
        preloadInfo.Add(("Crossroads_ShamanTemple", "Shiny Item"));
    }

    internal override void AfterPreload(Dictionary<string, Dictionary<string, GameObject>> preloads)
    {
        _gameObject = preloads["Crossroads_ShamanTemple"]["Shiny Item"];
    }
}