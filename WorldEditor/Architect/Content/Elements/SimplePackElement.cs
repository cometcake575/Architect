using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Content.Elements;

/**
 * A placeable object that can be added by content pack
 */
public class SimplePackElement : AbstractPackElement
{
    private readonly GameObject _obj;
    [CanBeNull] private readonly Sprite _sprite;
        
    /**
     * <param name="obj">The object to add</param>
     * <param name="name">The name to register the object under</param>
     * <param name="category">The category to register the object under (a category will automatically be created if one by the specified name does not exist)</param>
     * <param name="sprite">The sprite to use for the object (will default to an attached renderer)</param>
     * <param name="weight">The weight to use for the object's order in the UI, higher weights come first</param>
     */
    public SimplePackElement(GameObject obj, string name, string category, [CanBeNull] Sprite sprite = null, int weight = 0) : base(name, category, weight)
    {
        _obj = obj;
        _sprite = sprite;
    }

    public override GameObject GetPrefab(bool flipped, float rotation)
    {
        return _obj; 
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }
}