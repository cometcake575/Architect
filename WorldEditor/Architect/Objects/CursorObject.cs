using System.Linq;
using System.Threading.Tasks;
using Architect.UI;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class CursorObject : SelectableObject
{
    internal static readonly CursorObject Instance = new();
    
    private CursorObject() : base("Cursor")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.Load("cursor");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        if (!first) return;
        
        var pl = PlacementManager.GetCurrentPlacements().FirstOrDefault(placement => placement.Touching(pos));
        if (pl == null) return;

        Task.Run(() => EditorUIManager.DisplayExtraInfo("Placement ID: " + pl.GetId()));
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