using System.Collections.Generic;
using System.Linq;
using Architect.Configuration;
using Architect.Content;
using Satchel.BetterMenus;
using UnityEngine;

namespace Architect.UI;

public static class ConfigurationScreen
{
    private static Menu _menuRef;
    
    public static MenuScreen GetScreen(MenuScreen modListMenu, WorldEditorGlobalSettings globalSettings)
    {
        string[] values =
        [
            "True",
            "False"
        ];
        var elements = new List<Element>
        {
            new TextPanel("Editor"),
            new HorizontalOption(
                name: "Allow Editing",
                description: "Enables the keybind to activate the editor",
                values: values,
                applySetting: i =>
                {
                    globalSettings.CanEnableEditing = i == 0;
                },
                loadSetting: () => globalSettings.CanEnableEditing ? 0 : 1
            ),
            new TextPanel("Keybinds"),
            new KeyBind(
                name: "Toggle Editor",
                playerAction: globalSettings.Keybinds.ToggleEditor
            ),
            new KeyBind(
                name: "Flip",
                playerAction: globalSettings.Keybinds.FlipItem
            ),
            new KeyBind(
                name: "Rotate",
                playerAction: globalSettings.Keybinds.RotateItem
            ),
            new KeyBind(
                name: "Use Unsafe Rotations",
                playerAction: globalSettings.Keybinds.UnsafeRotation
            ),
            new KeyBind(
                name: "Decrease Scale",
                playerAction: globalSettings.Keybinds.DecreaseScale
            ),
            new KeyBind(
                name: "Increase Scale",
                playerAction: globalSettings.Keybinds.IncreaseScale
            ),
            new KeyBind(
                name: "Lock Axis",
                playerAction: globalSettings.Keybinds.LockAxis
            ),
            new KeyBind(
                name: "Moving Object Preview",
                playerAction: globalSettings.Keybinds.TogglePreview
            ),
            new KeyBind(
                name: "Add Prefab",
                playerAction: globalSettings.Keybinds.AddPrefab
            ),
            new TextPanel("Content Pack Toggles"),
            new TextPanel("Disable packs you don't need to reduce startup time and memory usage, changes are applied when game is rebooted.\n\nThings will break if you disable a pack that is in use!")
            {
                FontSize = 20,
                Anchor = TextAnchor.UpperCenter
            }
        };
        if (Architect.UsingMultiplayer) elements.Insert(2, new HorizontalOption(
            name: "Collaboration Mode",
            description: "Shares edits as soon as they're made to allow working together online using HKMP",
            values: values,
            applySetting: i =>
            {
                globalSettings.CollaborationMode = i == 0;
            },
            loadSetting: () => globalSettings.CollaborationMode ? 0 : 1
        ));
        elements.AddRange(ContentPacks.Packs.Select(pack => new HorizontalOption(name: pack.GetName(), description: pack.GetDescription(), values: values, applySetting: i => { globalSettings.ContentPackSettings[pack.GetName()] = i == 0; }, loadSetting: () => globalSettings.ContentPackSettings[pack.GetName()] ? 0 : 1)));
        _menuRef ??= new Menu(
            name: Architect.Instance.Name,
            elements: elements.ToArray()
        );
        return _menuRef.GetMenuScreen(modListMenu);
    }
}