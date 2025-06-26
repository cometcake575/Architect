using System.Collections.Generic;
using System.Linq;
using Architect.Configuration;
using Architect.Content;
using Satchel.BetterMenus;
using Satchel.BetterMenus.Config;
using UnityEngine;

namespace Architect.UI;

public static class ConfigurationScreen
{
    private static Menu _menuRef;
    
    public static MenuScreen GetScreen(MenuScreen modListMenu, WorldEditorGlobalSettings globalSettings)
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
            new TextPanel("Content Pack Toggles"),
            new TextPanel("Disable packs you don't need to reduce startup time and memory usage, changes are applied when game is rebooted.\n\nThings will break if you disable a pack that is in use!")
            {
                FontSize = 20,
                Anchor = TextAnchor.UpperCenter
            }
        };
        elements.AddRange(ContentPacks.Packs.Select(pack => new HorizontalOption(name: pack.GetName(), description: pack.GetDescription(), values: values, applySetting: i => { globalSettings.ContentPackSettings[pack.GetName()] = i == 0; }, loadSetting: () => globalSettings.ContentPackSettings[pack.GetName()] ? 0 : 1)).Cast<Element>());
        _menuRef ??= new Menu(
            name: Architect.Instance.Name,
            elements: elements.ToArray()
        );
        return _menuRef.GetMenuScreen(modListMenu);
    }
}