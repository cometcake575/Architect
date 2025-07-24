using System.Globalization;
using System.Linq;
using Architect.UI;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class PickObject : SelectableObject
{
    internal static readonly PickObject Instance = new();
    
    private PickObject() : base("Pick")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.Load("pick");
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        var pl = PlacementManager.FindClickedObject(pos);
        if (pl == null) return;

        EditorUIManager.SelectedItem = pl.GetPlaceableObject();
        LoadConfig(pl);
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

    public static void LoadConfig(ObjectPlacement pl)
    {
        foreach (var config in pl.Config) EditorUIManager.ConfigValues[config.GetName()] = config;
        foreach (var broadcaster in pl.Broadcasters) EditorUIManager.Broadcasters.Add(broadcaster);
        foreach (var receiver in pl.Receivers) EditorUIManager.Receivers.Add(receiver);
        
        EditorUIManager.RefreshSelectedItem(false);
        
        EditorManager.Scale = pl.Scale;
        EditorManager.Rotation = pl.Rotation;
        EditorManager.IsFlipped = pl.Flipped;
        
        EditorUIManager.ScaleChoice.Text = pl.Scale.ToString(CultureInfo.InvariantCulture);
        EditorUIManager.RotationChoice.Text = pl.Rotation.ToString(CultureInfo.InvariantCulture);
    }
}