using InControl;

namespace Architect.Configuration;

public class WorldEditorKeyBinds : PlayerActionSet
{
    public PlayerAction ToggleEditor;
    public PlayerAction FlipItem;
    public PlayerAction RotateItem;
    public PlayerAction IncreaseScale;
    public PlayerAction DecreaseScale;

    public WorldEditorKeyBinds()
    {
        ToggleEditor = CreatePlayerAction("EditToggle");
        FlipItem = CreatePlayerAction("FlipItem");
        RotateItem = CreatePlayerAction("RotateItem");
        IncreaseScale = CreatePlayerAction("IncreaseScale");
        DecreaseScale = CreatePlayerAction("DecreaseScale");
        
        ToggleEditor.AddDefaultBinding(Key.E);
        FlipItem.AddDefaultBinding(Key.F);
        RotateItem.AddDefaultBinding(Key.R);
        DecreaseScale.AddDefaultBinding(Key.Minus);
        IncreaseScale.AddDefaultBinding(Key.Equals);
    }
}