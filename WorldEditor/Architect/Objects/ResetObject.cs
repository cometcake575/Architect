using System.Globalization;
using System.Linq;
using Architect.Configuration;
using Architect.MultiplayerHook;
using Architect.UI;
using Architect.Util;
using UnityEngine;

namespace Architect.Objects;

internal class ResetObject : SelectableObject
{
    internal static readonly ResetObject Instance = new();
    
    private ResetObject() : base("Reset - Cannot be undone!")
    {
        _sprite = PrepareSprite();
    }

    private static Sprite PrepareSprite()
    {
        return ResourceUtils.Load("reset_rocket");
    }

    private static float _resetTime;

    public static void RestartDelay()
    {
        _resetTime = 0;
    }

    public override void OnClickInWorld(Vector3 pos, bool first)
    {
        _resetTime += Time.deltaTime;

        if (_resetTime >= 3)
        {
            var id = GameManager.instance.sceneName;
            ResetRoom(id);
            
            if (!Architect.UsingMultiplayer || !Architect.GlobalSettings.CollaborationMode) return;
            HkmpHook.ClearRoom(id);
        }
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

    public static void ResetRoom(string sceneName)
    {
        if (sceneName == GameManager.instance.sceneName)
        {
            var placements = PlacementManager.GetCurrentPlacements();
            while (placements.Count > 0) placements[0].Destroy();
        }
        else SceneSaveLoader.SaveScene(sceneName, []);
    }
}