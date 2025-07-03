using System.Linq;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class DragObject : SelectableObject
{
    internal static readonly DragObject Instance = new();
    
    private DragObject() : base("Drag")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.Load("drag");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        if (!first) return;
        
        var pl = PlacementManager.GetCurrentPlacements().FirstOrDefault(placement => placement.Touching(pos));
        if (pl == null) return;
        EditorManager.SetDraggedItem(pl);
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