using InControl;

namespace Architect.utils;

public class WorldEditorKeyBinds : PlayerActionSet
{
    public PlayerAction ToggleEditor;
    public PlayerAction FlipItem;
    public PlayerAction RotateItem;

    public WorldEditorKeyBinds()
    {
        ToggleEditor = CreatePlayerAction("EditToggle");
        FlipItem = CreatePlayerAction("FlipItem");
        RotateItem = CreatePlayerAction("RotateItem");
        
        ToggleEditor.AddDefaultBinding(Key.E);
        FlipItem.AddDefaultBinding(Key.F);
        RotateItem.AddDefaultBinding(Key.R);
    }
}