using System.Collections.Generic;
using Architect.Attributes;
using Architect.Util;

namespace Architect.Objects;

public static class PlacementManager
{
    public static List<ObjectPlacement> GetCurrentPlacements()
    {
        if (!Architect.GlobalSettings.Edits.ContainsKey(GameManager.instance.sceneName))
            Architect.GlobalSettings.Edits[GameManager.instance.sceneName] = new List<ObjectPlacement>();
        return Architect.GlobalSettings.Edits[GameManager.instance.sceneName];
    }

    private static void LoadPlacements()
    {
        foreach (var placement in GetCurrentPlacements())
        {
            if (EditorManager.IsEditing) placement.PlaceGhost();
            else placement.SpawnObject();
        }
    }

    internal static void Initialize()
    {
        On.HeroController.SceneInit += (orig, self) =>
        {
            LoadPlacements();
            orig(self);
        };

        On.HeroController.OnLevelUnload += (orig, self) =>
        {
            EventManager.ClearEventReceivers();
            orig(self);
        };
    }
}