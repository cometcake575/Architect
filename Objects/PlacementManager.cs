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
    private static LevelData _currentPlacements;
    private static tk2dTileMap _tileMap;

    public static readonly Dictionary<string, GameObject> Objects = [];

    public static List<ObjectPlacement> GetCurrentPlacements()
    {
        return GetCurrentLevel().Placements;
    }

    public static tk2dTileMap GetTilemap()
    {
        if (!_tileMap) _tileMap = Object.FindObjectOfType<tk2dTileMap>();
        return _tileMap;
    }

    public static LevelData GetCurrentLevel()
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
        var ld = GetCurrentLevel();
        foreach (var placement in ld.Placements.Where(placement => placement.GetPlaceableObject() != null))
            if (EditorManager.IsEditing) placement.PlaceGhost();
            else
                Objects[placement.GetId()] = placement.SpawnObject();

        var map = GetTilemap();
        if (!map) return;
        foreach (var (x, y) in ld.TileChanges)
        {
            if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
            else map.ClearTile(x, y, 0);
        }
        map.Build();
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