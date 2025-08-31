using System.Collections.Generic;
using System.Linq;
using Architect.Content;
using Architect.Objects;
using Architect.Storage;
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
                "Allow Editing",
                "Enables the keybind to activate the editor",
                values,
                i => { globalSettings.CanEnableEditing = i == 0; },
                () => globalSettings.CanEnableEditing ? 0 : 1
            ),
            new HorizontalOption(
                "Test Mode",
                "Stops the game from storing persistent data such as enemies being killed",
                values,
                i => { globalSettings.TestMode = i == 0; },
                () => globalSettings.TestMode ? 0 : 1
            ),
            new TextPanel(""),
            new TextPanel("Keybinds"),
            new KeyBind(
                "Toggle Editor",
                globalSettings.Keybinds.ToggleEditor
            ),
            new MenuRow([
                    new KeyBind(
                        "Undo",
                        globalSettings.Keybinds.Undo
                    ),
                    new KeyBind(
                        "Redo",
                        globalSettings.Keybinds.Redo
                    )
                ],
                "undo_redo"),
            new MenuRow([
                    new KeyBind(
                        "Copy Selection",
                        globalSettings.Keybinds.Copy
                    ),
                    new KeyBind(
                        "Paste Selection",
                        globalSettings.Keybinds.Paste
                    )
                ],
                "copy_paste"),
            new MenuRow([
                    new KeyBind(
                        "Rotate",
                        globalSettings.Keybinds.RotateItem
                    ),
                    new KeyBind(
                        "Use Unsafe Rotations",
                        globalSettings.Keybinds.UnsafeRotation
                    )
                ],
                "rotations"),
            new MenuRow([
                    new KeyBind(
                        "Decrease Scale",
                        globalSettings.Keybinds.DecreaseScale
                    ),
                    new KeyBind(
                        "Increase Scale",
                        globalSettings.Keybinds.IncreaseScale
                    )
                ],
                "scale"),
            new MenuRow([
                    new KeyBind(
                        "Flip",
                        globalSettings.Keybinds.FlipItem
                    ),
                    new KeyBind(
                        "Lock Axis",
                        globalSettings.Keybinds.LockAxis
                    )
                ],
                "flip_lock"),
            new MenuRow([
                    new KeyBind(
                        "Moving Object Preview",
                        globalSettings.Keybinds.TogglePreview
                    ),
                    new KeyBind(
                        "Add Prefab",
                        globalSettings.Keybinds.AddPrefab
                    )
                ],
                "preview_prefab"),

            new TextPanel(""),
            new TextPanel("Management"),
            new MenuButton("Delete All Edits",
                "",
                _ =>
                {
                    if (GameManager.instance.IsGameplayScene())
                    {
                        ResetObject.ResetRoom(GameManager.instance.sceneName);
                        PlacementManager.InvalidateCache();
                    }

                    SceneSaveLoader.WipeAllScenes();
                }),

            new TextPanel(""),
            new TextPanel("Content Pack Toggles"),
            new TextPanel(
                "Disable packs you don't need to reduce startup time and memory usage, changes are applied when game is rebooted.\n\nThings will break if you disable a pack that is in use!")
            {
                FontSize = 20,
                Anchor = TextAnchor.UpperCenter
            }
        };
        if (Architect.UsingMultiplayer)
            elements.Insert(3, new HorizontalOption(
                "Collaboration Mode",
                "Shares edits across of an HKMP server as soon as they're made, to allow working together online",
                values,
                i => { globalSettings.CollaborationMode = i == 0; },
                () => globalSettings.CollaborationMode ? 0 : 1
            ));
        elements.AddRange(ContentPacks.Packs.Select(pack => new HorizontalOption(pack.GetName(), pack.GetDescription(),
            values, i => { globalSettings.ContentPackSettings[pack.GetName()] = i == 0; },
            () => globalSettings.ContentPackSettings[pack.GetName()] ? 0 : 1)));
        _menuRef ??= new Menu(
            Architect.Instance.Name,
            elements.ToArray()
        );

        var ms = _menuRef.GetMenuScreen(modListMenu);
        var scroll = ms.content.transform.Find("Scrollbar").transform;
        var pos = scroll.position;
        pos.x += 1;
        scroll.position = pos;

        return ms;
    }
}