using Architect.Util;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal class CustomSpriteElement(
    string scene,
    string path,
    string name,
    string spritePath,
    string category)
    : GInternalPackElement(scene, path, name, category, 0)
{
    private readonly Sprite _sprite = ResourceUtils.LoadInternal(spritePath, ppu:64);

    public override Sprite GetSprite()
    {
        return _sprite;
    }
}