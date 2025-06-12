using System.Collections.Generic;
using Architect.Attributes;
using Architect.Util;

namespace Architect.Objects;

public static class PlacementManager
{
    internal static List<ObjectPlacement> Placements => Architect.GlobalSettings.Edits.GetPlacements(GameManager.instance.sceneName);

    private static void LoadPlacements()
    {
        foreach (var placement in Placements)
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