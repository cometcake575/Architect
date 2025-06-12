using System.Collections.Generic;
using Architect.Configuration;
using Satchel.BetterMenus;

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
            )
        };
        _menuRef ??= new Menu(
            name: Architect.Instance.Name,
            elements: elements.ToArray()
        );
        return _menuRef.GetMenuScreen(modListMenu);
    }
}