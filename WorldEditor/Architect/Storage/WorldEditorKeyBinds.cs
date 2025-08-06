using InControl;

namespace Architect.Storage;

public class WorldEditorKeyBinds : PlayerActionSet
{
    public PlayerAction ToggleEditor;
    public PlayerAction Undo;
    public PlayerAction Redo;
    public PlayerAction Copy;
    public PlayerAction Paste;
    public PlayerAction FlipItem;
    public PlayerAction RotateItem;
    public PlayerAction UnsafeRotation;
    public PlayerAction IncreaseScale;
    public PlayerAction DecreaseScale;
    public PlayerAction LockAxis;
    public PlayerAction TogglePreview;
    public PlayerAction AddPrefab;

    public WorldEditorKeyBinds()
    {
        ToggleEditor = CreatePlayerAction("EditToggle");
        Undo = CreatePlayerAction("Undo");
        Redo = CreatePlayerAction("Redo");
        Copy = CreatePlayerAction("Copy");
        Paste = CreatePlayerAction("Paste");
        FlipItem = CreatePlayerAction("FlipItem");
        RotateItem = CreatePlayerAction("RotateItem");
        UnsafeRotation = CreatePlayerAction("UnsafeRotation");
        IncreaseScale = CreatePlayerAction("IncreaseScale");
        DecreaseScale = CreatePlayerAction("DecreaseScale");
        LockAxis = CreatePlayerAction("LockAxis");
        TogglePreview = CreatePlayerAction("TogglePreview");
        AddPrefab = CreatePlayerAction("AddPrefab");
        
        ToggleEditor.AddDefaultBinding(Key.E);
        Undo.AddDefaultBinding(Key.Z);
        Redo.AddDefaultBinding(Key.Y);
        Copy.AddDefaultBinding(Key.C);
        Paste.AddDefaultBinding(Key.V);
        FlipItem.AddDefaultBinding(Key.F);
        RotateItem.AddDefaultBinding(Key.R);
        UnsafeRotation.AddDefaultBinding(Key.LeftAlt);
        DecreaseScale.AddDefaultBinding(Key.Minus);
        IncreaseScale.AddDefaultBinding(Key.Equals);
        LockAxis.AddDefaultBinding(Key.RightShift);
        TogglePreview.AddDefaultBinding(Key.P);
        AddPrefab.AddDefaultBinding(Key.Return);
    }
}