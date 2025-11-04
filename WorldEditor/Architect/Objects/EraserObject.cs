using System.Collections.Generic;
using System.Linq;
using Architect.MultiplayerHook;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class EraserObject : SelectableObject
{
    internal static readonly EraserObject Instance = new();

    private readonly Sprite _sprite;

    private EraserObject() : base("Eraser")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.LoadInternal("eraser");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        List<ObjectPlacement> placements = [];
        placements.AddRange(EditorManager.Dragged.Select(obj => obj.Placement));
        EditorManager.Dragged.Clear();

        var placement = PlacementManager.FindClickedObject(pos);
        if (placement != null && !placements.Contains(placement)) placements.Add(placement);

        if (placements.Count == 0) return;

        UndoManager.PerformAction(new EraseObject(placements));

        foreach (var pl in placements)
        {
            pl.StopDragging();
            pl.Destroy();

            if (!Architect.UsingMultiplayer || !Architect.GlobalSettings.CollaborationMode) continue;
            var id = pl.GetId();
            HkmpHook.Erase(id, GameManager.instance.sceneName);
        }
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