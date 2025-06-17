using System.Linq;
using Architect.Content.Groups;
using Architect.Objects;
using Architect.UI;
using MagicUI.Core;
using Modding;
using UnityEngine;

namespace Architect.Util;

public static class EditorManager
{
    internal static bool IsEditing;
    internal static bool IsFlipped;
    internal static int Rotation;
    internal static float Scale = 1;

    private static Vector3 _posToLoad;
    private static bool _loadPos;
    private static bool _needsReload;
    
    internal static Camera GameCamera;
    
    public static void Initialize()
    {
        ModHooks.HeroUpdateHook += EditorUpdate;

        On.GameManager.EnterHero += LoadStoredPosition;

        On.QuitToMenu.Start += (orig, self) =>
        {
            IsEditing = false;
            return orig(self);
        };
        
        On.HeroController.CanAttack += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanCast += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanNailCharge += (orig, self) => !IsEditing && orig(self);
        
        On.HeroController.CanNailArt += (orig, self) => !IsEditing && orig(self);
        
        On.HeroController.CanTakeDamage += (orig, self) => orig(self) && !IsEditing;
        
        ModHooks.CursorHook += () => { Cursor.visible = GameManager.instance.isPaused || (IsEditing && EditorUIManager.SelectedItem is not PlaceableObject); };
    }

    private static void LoadStoredPosition(On.GameManager.orig_EnterHero orig, GameManager self, bool search)
    {
        if (_loadPos)
        {
            self.entryGateName = null;
            HeroController.instance.transform.position = _posToLoad;
            _loadPos = false;
        }
        orig(self, search);
    }

    private static void EditorUpdate()
    {
        if (!Architect.GlobalSettings.CanEnableEditing) return;
        
        if (!GameCamera) GameCamera = Camera.allCameras.FirstOrDefault(cam => cam.gameObject.name.Equals("tk2dCamera"));
        
        var paused = GameManager.instance.isPaused;

        if (_needsReload && !paused && !HeroController.instance.controlReqlinquished)
        {
            _needsReload = false;
            ReloadScene();
        }
        
        CheckToggle(paused);
        
        CursorItem.TryRefresh(paused || !IsEditing);
        
        if (!IsEditing) return;
        HeroController.instance.GetComponent<Rigidbody2D>().velocity.Set(0, 0);

        // Checks if the selected item is placeable
        if (EditorUIManager.SelectedItem is PlaceableObject placeable)
        { 
            // Was the 'flip item' keybind pressed
            if (Architect.GlobalSettings.Keybinds.FlipItem.WasPressed)
            {
                // Inverts flip
                IsFlipped = !IsFlipped;
                // Refresh cursor representation
                CursorItem.NeedsRefreshing = true;
            }

            // Checks that the selected item is rotatable
            if (placeable.PackElement.GetRotationGroup() != RotationGroup.None)
            {
                // If the rotation group is full 360, rotate when held down, otherwise only rotate when first pressed
                var pressedRotation =
                    placeable.PackElement.GetRotationGroup() == RotationGroup.All
                        ? Architect.GlobalSettings.Keybinds.RotateItem.IsPressed
                        : Architect.GlobalSettings.Keybinds.RotateItem.WasPressed;

                // Checks if it should rotate the selected item
                if (pressedRotation)
                {
                    var i = placeable.PackElement.GetRotationGroup() switch
                    {
                        RotationGroup.Vertical => 180,
                        RotationGroup.Three => 90,
                        RotationGroup.Four => 90,
                        RotationGroup.Eight => 45,
                        RotationGroup.All => 1,
                        _ => 0
                    };

                    Rotation = (Rotation + i) % 360;
                    if (placeable.PackElement.GetRotationGroup() == RotationGroup.Three && Rotation == 180) Rotation = 270;
                    
                    // Refresh cursor representation
                    CursorItem.NeedsRefreshing = true;
                }
            }

            var scaleDown = Architect.GlobalSettings.Keybinds.DecreaseScale.IsPressed;
            var scaleUp = Architect.GlobalSettings.Keybinds.IncreaseScale.IsPressed;

            if (scaleUp != scaleDown)
            {
                Scale = Mathf.Max(Scale + (scaleUp ? 1 : -1) * Time.deltaTime, 0.2f);
                CursorItem.NeedsRefreshing = true;
            }
        }

        HeroController.instance.AffectedByGravity(false);

        HeroActions actions = InputHandler.Instance.inputActions;

        if (paused) ShiftSelection(actions);
        else TryPlace();

        foreach (var element in EditorUIManager.PauseOptions)
        {
            element.Visibility = paused ? Visibility.Visible : Visibility.Hidden;
        }

        DoFreeMovement(actions);
    }

    private static void DoFreeMovement(HeroActions actions)
    {
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
    }

    private static void CheckToggle(bool paused)
    {
        if (paused) return;
        if (!Architect.GlobalSettings.Keybinds.ToggleEditor.WasPressed) return;
        if (!Architect.GlobalSettings.CanEnableEditing) return;
        if (HeroController.instance.controlReqlinquished) return;
        
        HeroController.instance.AffectedByGravity(IsEditing);
        IsEditing = !IsEditing;
        
        ReloadScene();
    }

    public static void ScheduleReloadScene()
    {
        _needsReload = true;
    }

    private static void ReloadScene()
    {
        _loadPos = true;
        _posToLoad = HeroController.instance.transform.position;
        GameManager.instance.LoadScene(GameManager.instance.sceneName);
    }

    private static void ShiftSelection(HeroActions actions)
    {
        var leftMenu = actions.left.WasPressed;
        var rightMenu = actions.right.WasPressed;
        if (leftMenu != rightMenu)
        {
            EditorUIManager.ShiftGroup(rightMenu ? 1 : -1);
        }
    }
    
    private static void TryPlace()
    {
        var b1 = Input.GetMouseButtonDown(0);
        var b2 = Input.GetMouseButton(0);

        if (!b1 && !b2) return;
        
        if (EditorUIManager.SelectedItem == null) return;
        if (!GameCamera) return;
        
        var pos = Input.mousePosition;
        pos.z = -GameCamera.transform.position.z;
        EditorUIManager.SelectedItem.OnClickInWorld(GameCamera.ScreenToWorldPoint(pos), b1);
    }
}