using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class DragObject : SelectableObject
{
    internal static readonly DragObject Instance = new();

    private readonly Sprite _sprite;

    private DragObject() : base("Drag")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.LoadInternal("drag");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        if (!first) return;

        var pl = PlacementManager.FindClickedObject(pos);
        if (pl == null)
        {
            EditorManager.StartGroupSelect(pos);
            return;
        }

        EditorManager.AddDraggedItem(pl, pos, !Input.GetKey(KeyCode.LeftControl));
    }

    public override bool IsFavourite()
    {
        return false;
    }

    public override Sprite GetSprite()
    {
        return _sprite;
    }

    public override int GetWeight()
    {
        return 0;
    }
}