using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Util;

namespace Architect.Objects;

public static class PlacementManager
{
    private static string _sceneName;
    private static List<ObjectPlacement> _currentPlacements;

    public static List<ObjectPlacement> GetCurrentPlacements()
    {
        var sceneName = GameManager.instance.sceneName;
        if (_sceneName == sceneName) return _currentPlacements;

        _sceneName = sceneName;
        
        if (Architect.GlobalSettings.Edits.TryGetValue(_sceneName, out _currentPlacements)) return _currentPlacements;
        
        _currentPlacements = new List<ObjectPlacement>();
        Architect.GlobalSettings.Edits[_sceneName] = _currentPlacements;

        return _currentPlacements;
    }

    private static void LoadPlacements()
    {
        foreach (var placement in GetCurrentPlacements().Where(placement => placement.GetPlaceableObject() != null))
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