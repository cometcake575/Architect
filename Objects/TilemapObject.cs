using System.Collections.Generic;
using System.Linq;
using Architect.MultiplayerHook;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class TilemapObject : SelectableObject
{
    internal static readonly TilemapObject Instance = new();

    private static readonly List<(int, int)> TileFlips = [];
    
    private static (int, int) _lastPos = (-1, -1);
    private static bool _lastEmpty;

    private readonly Sprite _sprite;

    private TilemapObject() : base("Tilemap Editor")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.LoadInternal("tilemap");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        var map = PlacementManager.GetTilemap();
        if (!map || !map.GetTileAtPosition(pos, out var x, out var y)) return;

        var xyPos = (x, y);
        if (_lastPos == xyPos && !first) return;
        _lastPos = xyPos;

        var empty = map.GetTile(x, y, 0) == -1;
        if (first) _lastEmpty = empty;
        else if (_lastEmpty != empty) return;

        if (empty) map.SetTile(x, y, 0, 0);
        else map.ClearTile(x, y, 0);
        map.Build();

        PlacementManager.GetCurrentLevel().ToggleTile(xyPos);
        
        TileFlips.Add(xyPos);
    }

    public static void Release()
    {
        if (TileFlips.Count == 0) return;
        UndoManager.PerformAction(new ToggleTile(TileFlips.ToList(), _lastEmpty));

        if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
        {
            HkmpHook.TilemapChange(GameManager.instance.sceneName, TileFlips, _lastEmpty);
        }
        
        TileFlips.Clear();
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