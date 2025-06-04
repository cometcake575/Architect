using UnityEngine;
using Architect.utils;

namespace Architect.objects;

internal class EraserObject : SelectableObject
{
    internal static EraserObject Instance = new();
    
    protected EraserObject() : base("Eraser")
    {
        _sprite = PrepareSprite();
    }

    private Sprite PrepareSprite()
    {
        return WESpriteUtils.Load("eraser");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        ObjectPlacement pl = null;
        foreach (ObjectPlacement placement in ObjectPlacement.Placements)
        {
            if (placement.Touching(pos))
            {
                pl = placement;
                break;
            }
        }

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
}