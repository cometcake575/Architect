using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Configuration;
using Architect.Content.Elements.Custom;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

public static class PlacementManager
{
    private static string _sceneName;
    private static List<ObjectPlacement> _currentPlacements;

    public static List<ObjectPlacement> GetCurrentPlacements()
    {
        var sceneName = GameManager.instance.sceneName;
        if (_sceneName == sceneName) return _currentPlacements;
        
        if (EditorManager.IsEditing) SceneSaveLoader.SaveScene(_sceneName, _currentPlacements);
        _sceneName = sceneName;
        _currentPlacements = SceneSaveLoader.LoadScene(sceneName);

        return _currentPlacements;
    }

    public static void InvalidateCache()
    {
        _sceneName = "Invalid";
    }

    public static readonly Dictionary<string, GameObject> Objects = [];

    private static void LoadPlacements()
    {
        Objects.Clear();
        CustomObjects.PlayerListeners.Clear();
        foreach (var placement in GetCurrentPlacements().Where(placement => placement.GetPlaceableObject() != null))
        {
            if (EditorManager.IsEditing) placement.PlaceGhost();
            else
            {
                Objects[placement.GetId()] = placement.SpawnObject();
            }
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