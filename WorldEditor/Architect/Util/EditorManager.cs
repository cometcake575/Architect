using System;
using System.Globalization;
using System.Linq;
using Architect.Content.Groups;
using Architect.MultiplayerHook;
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
    internal static float Rotation;
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

        On.HeroController.CanDreamNail += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanSuperDash += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanCast += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanNailCharge += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanNailArt += (orig, self) => !IsEditing && orig(self);

        On.HeroController.CanTakeDamage += (orig, self) => orig(self) && !IsEditing;

        ModHooks.CursorHook += () =>
        {
            Cursor.visible = GameManager.instance.isPaused ||
                             (IsEditing && EditorUIManager.SelectedItem is not PlaceableObject);
        };
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
        var paused = GameManager.instance.isPaused;
        
        if (_needsReload && !paused && !HeroController.instance.controlReqlinquished)
        {
            _needsReload = false;
            ReloadScene();
        }

        if (!Architect.GlobalSettings.CanEnableEditing) return;

        if (_dragged != null)
        {
            if (paused || Input.GetMouseButtonUp(0))
            {
                ReleaseDraggedItem();
            }
            else
            {
                _dragged.Move(GetWorldPos(Input.mousePosition));
            }
        }

        if (!GameCamera) GameCamera = Camera.allCameras.FirstOrDefault(cam => cam.gameObject.name.Equals("tk2dCamera"));

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
            var group = Architect.GlobalSettings.Keybinds.UnsafeRotation.IsPressed
                ? RotationGroup.All
                : placeable.PackElement.GetRotationGroup();
            if (group != RotationGroup.None)
            {
                // If the rotation group is full 360, rotate when held down, otherwise only rotate when first pressed
                var pressedRotation =
                    group == RotationGroup.All
                        ? Architect.GlobalSettings.Keybinds.RotateItem.IsPressed
                        : Architect.GlobalSettings.Keybinds.RotateItem.WasPressed;

                // Checks if it should rotate the selected item
                if (pressedRotation)
                {
                    float i;
                    if (group == RotationGroup.All)
                    {
                        i = Time.deltaTime * 60;
                    }
                    else
                        i = group switch
                        {
                            RotationGroup.Vertical => 180,
                            RotationGroup.Three => 90,
                            RotationGroup.Four => 90,
                            RotationGroup.Eight => 45,
                            _ => 0
                        };

                    Rotation = (Rotation + i) % 360;
                    if (group == RotationGroup.Three && Mathf.Approximately(Rotation, 180)) Rotation = 270;
                    EditorUIManager.RotationChoice.Text = Rotation.ToString(CultureInfo.InvariantCulture);

                    // Refresh cursor representation
                    CursorItem.NeedsRefreshing = true;
                }
            }

            var scaleDown = Architect.GlobalSettings.Keybinds.DecreaseScale.IsPressed;
            var scaleUp = Architect.GlobalSettings.Keybinds.IncreaseScale.IsPressed;

            if (scaleUp != scaleDown)
            {
                Scale = Mathf.Max(Scale + (scaleUp ? 1 : -1) * Time.deltaTime, 0.1f);
                EditorUIManager.ScaleChoice.Text = Scale.ToString(CultureInfo.InvariantCulture);
                CursorItem.NeedsRefreshing = true;
            }
        }

        HeroController.instance.AffectedByGravity(false);

        var actions = InputHandler.Instance.inputActions;

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
        var up = actions.up.IsPressed;
        var down = actions.down.IsPressed;
        var left = actions.left.IsPressed;
        var right = actions.right.IsPressed;

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
        var fsm = HeroController.instance.gameObject.LocateMyFSM("Surface Water");
        if (fsm.ActiveStateName == "Idle") fsm.SetState("Regain Control");

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

        var pos = GetWorldPos(Input.mousePosition);
        EditorUIManager.SelectedItem.OnClickInWorld(pos, b1);

        if (EditorUIManager.SelectedItem is not PlaceableObject) return;
        if (Architect.GlobalSettings.Keybinds.LockAxis.IsPressed && HasValidLastPos) return;

        _lastX = pos.x;
        _lastY = pos.y;
        HasValidLastPos = true;
    }

    private static float _lastX;
    private static float _lastY;

    private static bool HasValidLastPos
    {
        get => _validLastPosScene == GameManager.instance.sceneName;
        set => _validLastPosScene = value ? GameManager.instance.sceneName : "";
    }

    private static string _validLastPosScene;

    public static Vector3 GetWorldPos(Vector3 mousePosition)
    {
        mousePosition.z = -GameCamera.transform.position.z;
        var pos = GameCamera.ScreenToWorldPoint(mousePosition);

        if (Architect.GlobalSettings.Keybinds.LockAxis.IsPressed && HasValidLastPos)
        {
            if (Math.Abs(_lastX - pos.x) > Math.Abs(_lastY - pos.y)) pos.y = _lastY;
            else pos.x = _lastX;
        }

        return pos;
    }

    private static ObjectPlacement _dragged;

    public static void SetDraggedItem(ObjectPlacement placement)
    {
        _dragged = placement;
    }

    public static void ReleaseDraggedItem()
    {
        if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
        {
            var id = _dragged.GetId();
            HkmpHook.Update(id, GameManager.instance.sceneName, _dragged.GetPos());
        }

        _dragged = null;
    }
}