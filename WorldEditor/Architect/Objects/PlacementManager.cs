using System.Collections.Generic;
using System.Linq;
using Architect.Attributes;
using Architect.Content.Elements.Custom;
using Architect.Storage;
using Architect.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace Architect.Objects;

public static class PlacementManager
{
    private static string _sceneName;
    private static List<ObjectPlacement> _currentPlacements;

    public static readonly Dictionary<string, GameObject> Objects = [];

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

    private static void LoadPlacements()
    {
        Objects.Clear();
        CustomObjects.PlayerListeners.Clear();
        foreach (var placement in GetCurrentPlacements().Where(placement => placement.GetPlaceableObject() != null))
            if (EditorManager.IsEditing) placement.PlaceGhost();
            else
                Objects[placement.GetId()] = placement.SpawnObject();
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

    [CanBeNull]
    public static ObjectPlacement FindClickedObject(Vector3 mousePos, bool ignoreLock = false)
    {
        return GetCurrentPlacements().FirstOrDefault(placement => (ignoreLock || !placement.Locked) 
                                                                  && placement.Touching(mousePos));
    }
}