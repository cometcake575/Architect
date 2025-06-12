using System.Linq;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class EraserObject : SelectableObject
{
    internal static readonly EraserObject Instance = new();
    
    private EraserObject() : base("Eraser")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return WeSpriteUtils.Load("eraser");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        var pl = PlacementManager.Placements.FirstOrDefault(placement => placement.Touching(pos));
        pl?.Destroy();
    }

    public override bool IsFavourite()
    {
        return false;
    }

    private readonly Sprite _sprite;

    public override Sprite GetSprite()
    {
        return _sprite;
    }

    public override int GetWeight()
    {
        return 0;
    }
}