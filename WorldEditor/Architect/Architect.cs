using System;
using System.Collections.Generic;
using System.Reflection;
using Architect.Attributes;
using Architect.Content;
using Architect.Content.Elements.Custom;
using Architect.Content.Elements.Custom.SaL;
using Architect.MultiplayerHook;
using Architect.Objects;
using Architect.Storage;
using Architect.UI;
using Architect.Util;
using GlobalEnums;
using MagicUI.Core;
using Modding;
using Satchel;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Architect;

public class Architect : Mod, IGlobalSettings<WorldEditorGlobalSettings>, ICustomMenuMod
{
    public static bool UsingMultiplayer;

    public static GameObject ArrowPromptNew;

    private static LayoutRoot _editorLayout;

    private static LayoutRoot _menuLayout;

    // Blank sprite texture
    public static readonly Sprite BlankSprite =
        Sprite.Create(Texture2D.normalTexture, new Rect(0, 0, 0, 0), new Vector2());

    public Architect() : base("Architect")
    {
    }

    public static Architect Instance { get; private set; }

    internal static WorldEditorGlobalSettings GlobalSettings { get; private set; } = new();

    public override List<(string, string)> GetPreloadNames()
    {
        ContentPacks.PreloadInternalPacks();
        var preloadValues = ContentPacks.GetPreloadValues();

        preloadValues.Add(("Crossroads_47", "RestBench"));

        return preloadValues;
    }

    public override string GetVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Log("Initializing");
        Instance = this;
        Log("Initialized");

        ArrowPromptNew = preloadedObjects["Crossroads_47"]["RestBench"].LocateMyFSM("Bench Control")
            .GetAction<ShowPromptMarker>("In Range", 0).prefab.Value;

        foreach (var values in ContentPacks.GetPreloadValues())
            Object.DontDestroyOnLoad(preloadedObjects[values.Item1][values.Item2]);

        ContentPacks.AfterPreload(preloadedObjects);

        SceneSaveLoader.Initialize();

        CustomObjects.Initialize();
        RoomObjects.Initialize();

        EditorManager.Initialize();
        PlacementManager.Initialize();
        EventManager.InitializeBroadcasters();

        UndoManager.Initialize();

        if (ModHooks.GetMod("HKMP") is Mod)
        {
            HkmpHookInitializer.Initialize();
            UsingMultiplayer = true;
        }

        if (ModHooks.GetMod("ScatteredAndLost") is Mod) CustomSaL.Initialize();

        InitializeLayout();
    }

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        return ConfigurationScreen.GetScreen(modListMenu, GlobalSettings);
    }

    public bool ToggleButtonInsideMenu => throw new NotImplementedException();

    public void OnLoadGlobal(WorldEditorGlobalSettings s)
    {
        GlobalSettings = s;
    }

    public WorldEditorGlobalSettings OnSaveGlobal()
    {
        return GlobalSettings;
    }

    private static void InitializeLayout()
    {
        _editorLayout = new LayoutRoot(true, "Architect Editor");
        _editorLayout.Canvas.GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        
        EditorUIManager.Initialize(_editorLayout);

        _menuLayout = new LayoutRoot(true, "Architect Menu")
        {
            VisibilityCondition = () => UIManager.instance && UIManager.instance.menuState == MainMenuState.MAIN_MENU
        };
        LevelSharingManager.Initialize(_menuLayout);
    }
}