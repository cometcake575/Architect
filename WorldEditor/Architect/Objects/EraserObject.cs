using System.Linq;
using Architect.MultiplayerHook;
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
        return ResourceUtils.Load("eraser");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        var pl = PlacementManager.GetCurrentPlacements().FirstOrDefault(placement => placement.Touching(pos));
        if (pl == null) return;
        pl.Destroy();

        if (!Architect.UsingMultiplayer || !Architect.GlobalSettings.CollaborationMode) return;
        var id = pl.GetId();
        HkmpHook.Erase(id, GameManager.instance.sceneName);
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