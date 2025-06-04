using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using System.Reflection;
using JetBrains.Annotations;
using MagicUI.Core;
using Satchel.BetterMenus;
using UnityEngine;
using Architect.Content;
using Architect.objects;
using Architect.UI;
using Architect.utils;
using Object = UnityEngine.Object;

namespace Architect;

public class Architect : Mod, IGlobalSettings<WorldEditorGlobalSettings>, ICustomMenuMod
{
    public static Architect Instance { get; private set; }

    internal static bool IsEditing;
    internal static bool IsFlipped;
    internal static int Rotation;
    internal static Camera Target;

    [CanBeNull] private LayoutRoot _layout;

    public Architect() : base("Architect")
    {
    }

    public override List<(string, string)> GetPreloadNames()
    {
        ContentPacks.PreloadInternalPacks();
        return ContentPacks.GetPreloadValues();
    }

    private Vector3 _posToLoad;
    private bool _loadPos;

    public override string GetVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }

    internal Dictionary<string, Dictionary<string, GameObject>> PreloadedObjects;

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Log("Initializing");

        Instance = this;

        Log("Initialized");

        PreloadedObjects = preloadedObjects;

        foreach (var values in ContentPacks.GetPreloadValues())
        {
            Object.DontDestroyOnLoad(preloadedObjects[values.Item1][values.Item2]);
        }

        On.HeroController.CanAttack += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanCast += (orig, self) => !IsEditing && orig(self);

        ModHooks.AfterSavegameLoadHook += _ =>
        {
            IsEditing = false;

            _layout?.Destroy();
            
            _layout = new LayoutRoot(true, "Architect Layout");

            EditorUIManager.Initialize(_layout);
        };

        On.HeroController.SceneInit += (orig, self) =>
        {
            if (GlobalSettings.Edits.TryGetValue(GameManager.instance.sceneName, out var edit))
            {
                ObjectPlacement.LoadPlacements(edit);
                if (!IsEditing)
                {
                    ObjectPlacement.PlaceObjects();
                    ObjectPlacement.ClearPlacements();
                }
            }

            orig(self);
        };

        On.HeroController.LeaveScene += (orig, self, gate) =>
        {
            if (IsEditing)
            {
                if (ObjectPlacement.HasPlacements())
                {
                    var placements = ObjectPlacement.SavePlacements();
                    GlobalSettings.Edits[GameManager.instance.sceneName] = placements;
                }
                else GlobalSettings.Edits.Remove(GameManager.instance.sceneName);
            }

            ObjectPlacement.ClearPlacements();
            orig(self, gate);
        };

        On.GameManager.EnterHero += (orig, self, search) =>
        {
            if (_loadPos)
            {
                self.entryGateName = null;
                HeroController.instance.transform.position = _posToLoad;
                _loadPos = false;
            }

            orig(self, search);
        };

        On.HeroController.Pause += (orig, self) =>
        {
            orig(self);
            
            if (!IsEditing) return;
            if (ObjectPlacement.HasPlacements())
            {
                var placements = ObjectPlacement.SavePlacements();
                GlobalSettings.Edits[GameManager.instance.sceneName] = placements;
            }
            else GlobalSettings.Edits.Remove(GameManager.instance.sceneName);
        };

        On.QuitToMenu.Start += (orig, self) =>
        {
            IsEditing = false;
            return orig(self);
        };
        
        ModHooks.HeroUpdateHook += () =>
        {
            bool paused = GameManager.instance.isPaused;
            if (!HeroController.instance.controlReqlinquished && GlobalSettings.Keybinds.ToggleEditor.WasPressed &&
                _layout != null && !paused && GlobalSettings.CanEnableEditing)
            {
                if (IsEditing)
                {
                    if (ObjectPlacement.HasPlacements())
                    {
                        var placements = ObjectPlacement.SavePlacements();
                        GlobalSettings.Edits[GameManager.instance.sceneName] = placements;
                    }
                    else GlobalSettings.Edits.Remove(GameManager.instance.sceneName);
                }

                ObjectPlacement.ClearPlacements();
                _loadPos = true;
                _posToLoad = HeroController.instance.transform.position;
                GameManager.instance.LoadScene(GameManager.instance.sceneName);
                HeroController.instance.AffectedByGravity(IsEditing);
                IsEditing = !IsEditing;
            }
            
            if (!Target) Target = Camera.allCameras.FirstOrDefault(cam => cam.gameObject.name.Equals("tk2dCamera"));
            CursorItem.TryRefresh(paused || !IsEditing);
            if (!IsEditing) return;

            if (GlobalSettings.Keybinds.FlipItem.WasPressed)
            {
                IsFlipped = !IsFlipped;
                CursorItem.NeedsRefreshing = true;
            }
/*
            if (GlobalSettings.Keybinds.RotateItem.WasPressed)
            {
                Rotation = (Rotation + 90) % 360;
                CursorItem.NeedsRefreshing = true;
            }*/
            
            HeroController.instance.AffectedByGravity(false);

            HeroActions actions = InputHandler.Instance.inputActions;

            bool leftMenu = actions.left.WasPressed;
            bool rightMenu = actions.right.WasPressed;
            
            if (paused)
            {
                if (leftMenu != rightMenu)
                {
                    EditorUIManager.ShiftGroup(rightMenu ? 1 : -1);
                }
            }
            else if (EditorUIManager.SelectedItem != null)
            {
                bool b1 = Input.GetMouseButtonDown(0);
                bool b2 = Input.GetMouseButton(0);
                if (b1 || b2)
                {
                    if (Target)
                    {
                        Vector3 pos = Input.mousePosition;
                        pos.z = -Target.transform.position.z;
                        EditorUIManager.SelectedItem.OnClickInWorld(Target.ScreenToWorldPoint(pos), b1);
                    }
                }
            }

            foreach (var element in EditorUIManager.PauseOptions)
            {
                element.Visibility = paused ? Visibility.Visible : Visibility.Hidden;
            }

            bool up = actions.up.IsPressed;
            bool down = actions.down.IsPressed;
            bool left = actions.left.IsPressed;
            bool right = actions.right.IsPressed;

            HeroController.instance.current_velocity = Vector2.zero;
            if (up != down)
            {
                HeroController.instance.transform.position += (up ? Vector3.up : Vector3.down) * Time.deltaTime * 20;
            }

            if (left != right)
            {
                HeroController.instance.transform.position +=
                    (left ? Vector3.left : Vector3.right) * Time.deltaTime * 20;
            }
        };

        ModHooks.CursorHook += () => { Cursor.visible = GameManager.instance.isPaused || IsEditing; };
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

    private Menu _menuRef;

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        string[] values = {
            "True",
            "False"
        };
        var elements = new List<Element>
        {
            new TextPanel("Editor"),
            new HorizontalOption(
                name: "Allow Editing",
                description: "Enables the keybind to activate the editor",
                values: values,
                applySetting: i =>
                {
                    GlobalSettings.CanEnableEditing = i == 0;
                },
                loadSetting: () => GlobalSettings.CanEnableEditing ? 0 : 1
            ),
            new TextPanel("Keybinds"),
            new KeyBind(
                name: "Toggle Editor",
                playerAction: GlobalSettings.Keybinds.ToggleEditor
            ),
            new KeyBind(
                name: "Flip Item Orientation",
                playerAction: GlobalSettings.Keybinds.FlipItem
            )
            /*new KeyBind(
                name: "Rotate Item",
                playerAction: GlobalSettings.Keybinds.RotateItem
            ),*/
            //new TextPanel("Content Pack Toggles")
        };
        /*
        foreach (var pack in ContentPacks.Packs)
        {
            elements.Add(new HorizontalOption(
                name: pack.GetName(),
                description: pack.GetDescription(),
                values: values,
                applySetting: i =>
                {
                    GlobalSettings.ContentPackSettings[pack.GetName()] = i == 0;
                },
                loadSetting: () => GlobalSettings.ContentPackSettings[pack.GetName()] ? 0 : 1
            ));
        }
        */
        _menuRef ??= new Menu(
            name: Name,
            elements: elements.ToArray()
        );
        return _menuRef.GetMenuScreen(modListMenu);
    }

    public bool ToggleButtonInsideMenu => throw new NotImplementedException();
}