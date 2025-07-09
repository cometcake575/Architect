using InControl;

namespace Architect.Configuration;

public class WorldEditorKeyBinds : PlayerActionSet
{
    public PlayerAction ToggleEditor;
    public PlayerAction FlipItem;
    public PlayerAction RotateItem;
    public PlayerAction UnsafeRotation;
    public PlayerAction IncreaseScale;
    public PlayerAction DecreaseScale;
    public PlayerAction LockAxis;
    public PlayerAction TogglePreview;

    public WorldEditorKeyBinds()
    {
        ToggleEditor = CreatePlayerAction("EditToggle");
        FlipItem = CreatePlayerAction("FlipItem");
        RotateItem = CreatePlayerAction("RotateItem");
        UnsafeRotation = CreatePlayerAction("UnsafeRotation");
        IncreaseScale = CreatePlayerAction("IncreaseScale");
        DecreaseScale = CreatePlayerAction("DecreaseScale");
        LockAxis = CreatePlayerAction("LockAxis");
        TogglePreview = CreatePlayerAction("TogglePreview");
        
        ToggleEditor.AddDefaultBinding(Key.E);
        FlipItem.AddDefaultBinding(Key.F);
        RotateItem.AddDefaultBinding(Key.R);
        UnsafeRotation.AddDefaultBinding(Key.LeftAlt);
        DecreaseScale.AddDefaultBinding(Key.Minus);
        IncreaseScale.AddDefaultBinding(Key.Equals);
        LockAxis.AddDefaultBinding(Key.RightShift);
        TogglePreview.AddDefaultBinding(Key.P);
    }
}