using System.Collections.Generic;
using System.Linq;
using Architect.MultiplayerHook;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class LockObject : SelectableObject
{
    internal static readonly LockObject Instance = new();

    private readonly Sprite _sprite;

    private LockObject() : base("Lock")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.LoadInternal("lock");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        if (!first) return;
        var placement = PlacementManager.FindClickedObject(pos, true);
        placement?.ToggleLock();
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