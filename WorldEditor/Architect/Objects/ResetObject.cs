using System.Globalization;
using Architect.MultiplayerHook;
using Architect.Storage;
using Architect.UI;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class ResetObject : SelectableObject
{
    internal static readonly ResetObject Instance = new();

    private static float _resetTime;

    private static bool _resetting;

    private readonly Sprite _sprite;

    private ResetObject() : base("Reset - Cannot be undone!")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.LoadInternal("reset_rocket");
    }

    public static void RestartDelay()
    {
        if (_resetting)
        {
            _resetting = false;
            _resetTime = 0;
            EditorUIManager.SetText("");
        }
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        if (first) _resetting = true;
        if (!_resetting) return;

        _resetTime += Time.deltaTime;

        if (_resetTime >= 3)
        {
            RestartDelay();

            var id = GameManager.instance.sceneName;
            ResetRoom(id);

            if (!Architect.UsingMultiplayer || !Architect.GlobalSettings.CollaborationMode) return;
            HkmpHook.ClearRoom(id);
            return;
        }

        EditorUIManager.SetText(Mathf.Ceil(3 - _resetTime).ToString(CultureInfo.InvariantCulture));
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

    public static void ResetRoom(string sceneName)
    {
        if (sceneName == GameManager.instance.sceneName)
        {
            var level = PlacementManager.GetCurrentLevel();
            var placements = level.Placements;
            while (placements.Count > 0) placements[0].Destroy();

            var map = PlacementManager.GetTilemap();
            if (map)
            {
                foreach (var (x, y) in level.TileChanges)
                {
                    if (map.GetTile(x, y, 0) == -1) map.SetTile(x, y, 0, 0);
                    else map.ClearTile(x, y, 0);
                }
                map.Build();
            }
            level.TileChanges.Clear();

            UndoManager.ClearHistory();
        }
        else
        {
            SceneSaveLoader.SaveScene(sceneName, new LevelData([], []));
        }
    }
}