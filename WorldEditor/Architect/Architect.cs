using System;
using System.Collections.Generic;
using Modding;
using System.Reflection;
using Architect.Attributes;
using Architect.Configuration;
using UnityEngine;
using Architect.Content;
using Architect.Content.Elements.Custom;
using Architect.Content.Elements.Custom.SaL;
using Architect.MultiplayerHook;
using Architect.Objects;
using Architect.UI;
using Architect.Util;
using GlobalEnums;
using MagicUI.Core;
using Object = UnityEngine.Object;

namespace Architect;

public class Architect : Mod, IGlobalSettings<WorldEditorGlobalSettings>, ICustomMenuMod
{
    public static Architect Instance { get; private set; }

    public static bool UsingMultiplayer;
    
    public Architect() : base("Architect") { }

    public override List<(string, string)> GetPreloadNames()
    {
        ContentPacks.PreloadInternalPacks();
        return ContentPacks.GetPreloadValues();
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

        foreach (var values in ContentPacks.GetPreloadValues())
        {
            Object.DontDestroyOnLoad(preloadedObjects[values.Item1][values.Item2]);
        }
        
        ContentPacks.AfterPreload(preloadedObjects);
        
        SceneSaveLoader.Initialize();
        
        CustomObjects.Initialize();
        RoomObjects.Initialize();
        
        EditorManager.Initialize();
        PlacementManager.Initialize();
        EventManager.InitializeBroadcasters();
        
        if (ModHooks.GetMod("HKMP") is Mod)
        {
            HkmpHookInitializer.Initialize();
            UsingMultiplayer = true;
        }

        if (ModHooks.GetMod("ScatteredAndLost") is Mod)
        {
            CustomSaL.Initialize();
        }
        
        InitializeLayout();
    }

    private static LayoutRoot _editorLayout;
    
    private static LayoutRoot _menuLayout;
    
    private static void InitializeLayout()
    {
        _editorLayout = new LayoutRoot(true, "Architect Editor");
        EditorUIManager.Initialize(_editorLayout);
        
        _menuLayout = new LayoutRoot(true, "Architect Menu")
        {
            VisibilityCondition = () => UIManager.instance && UIManager.instance.menuState == MainMenuState.MAIN_MENU
        };
        MenuUIManager.Initialize(_menuLayout);
    }

    internal static WorldEditorGlobalSettings GlobalSettings { get; private set; } = new();

    public void OnLoadGlobal(WorldEditorGlobalSettings s)
    {
        GlobalSettings = s;
    }
    
    public WorldEditorGlobalSettings OnSaveGlobal()
    {
        return GlobalSettings;
    }
    
    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        return ConfigurationScreen.GetScreen(modListMenu, GlobalSettings);
    }

    public bool ToggleButtonInsideMenu => throw new NotImplementedException();
}