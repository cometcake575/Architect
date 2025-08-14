using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Architect.Attributes.Config;
using Architect.Category;
using Architect.Content.Groups;
using Architect.MultiplayerHook;
using Architect.Objects;
using Architect.Storage;
using Architect.UI;
using GlobalEnums;
using MagicUI.Core;
using Modding;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Architect.Util;

public static class EditorManager
{
    public static bool LostControlToCustomObject;
    
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

        On.GameManager.SaveGame += (orig, self) =>
        {
            if (IsEditing) SceneSaveLoader.SaveScene(self.sceneName, PlacementManager.GetCurrentPlacements());
            orig(self);
        };

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
        
        On.HeroController.CanDash += (orig, self) => !IsEditing && orig(self);

        On.HeroController.TakeDamage += (orig, self, go, side, amount, type) =>
        {
            if (IsEditing) return;
            orig(self, go, side, amount, type);
        };

        On.GameManager.EnterHero += (orig, self, search) =>
        {
            orig(self, search);
            if (!search) return;
            
            if (self.startedOnThisScene) return;
            if (string.IsNullOrEmpty(self.entryGateName)) return;
            var entry = GameObject.Find(self.entryGateName);
            if (entry && entry.activeInHierarchy) return;
            
            var hrm = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(obj => obj.GetComponentsInChildren<TransitionPoint>(true))
                .First();
            self.StartCoroutine(self.hero_ctrl.EnterScene(hrm, 0));
        };

        On.GameManager.FindEntryPoint += (orig, self, name, scene) =>
        {
            var point = orig(self, name, scene);
            if (!point.HasValue)
            {
                var hrm = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                    .SelectMany(obj => obj.GetComponentsInChildren<HazardRespawnMarker>(true))
                    .First();
                return hrm.transform.position;
            }

            return point;
        };
        
        On.HeroController.LocateSpawnPoint += (orig, self) =>
        {
            var point = orig(self);
            if (!point)
            {
                var hrm = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
                    .SelectMany(obj => obj.GetComponentsInChildren<HazardRespawnMarker>(true))
                    .First();
                return hrm.transform;
            }

            return point;
        };

        ModHooks.CursorHook += () =>
        {
            Cursor.visible = GameManager.instance.isPaused ||
                             (IsEditing && EditorUIManager.SelectedItem is not PlaceableObject);
        };

        
        On.PersistentBoolItem.Awake += (orig, self) =>
        {
            if (Architect.GlobalSettings.TestMode && self.gameObject.name.StartsWith("[Architect] "))
            {
                self.OnGetSaveState += (ref bool value) => value = false;
            }
            orig(self);
        };

        SetupGroupSelectionBox();
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

    private static bool _prevPaused;

    private static void EditorUpdate()
    {
        var paused = GameManager.instance.isPaused;

        if (_needsReload && !paused && !HeroController.instance.controlReqlinquished)
        {
            _needsReload = false;
            ReloadScene();
        }

        if (!Architect.GlobalSettings.CanEnableEditing) return;

        if (EditorUIManager.SelectedItem is DragObject)
        {
            if (Architect.GlobalSettings.Keybinds.Copy.WasPressed) DoCopy();
            if (Architect.GlobalSettings.Keybinds.Paste.WasPressed) DoPaste();
            
            RefreshGroupSelectionBox();
            if (Dragged.Count > 0)
            {
                if ((!Input.GetMouseButton(0) || paused) && _dragging) ReleaseDraggedItems(true);
                else if (!paused && Input.GetMouseButton(0) &&
                         (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
                {
                    var wp = GetWorldPos(Input.mousePosition);
                    if (!_dragging) BeginDragging(wp);
                    foreach (var dragged in Dragged) dragged.Placement.Move(wp + dragged.Offset);
                }
            }
        } else if (EditorUIManager.SelectedItem is not EraserObject) ReleaseDraggedItems(false);

        if (!GameCamera) GameCamera = GameCameras.instance.mainCamera;

        CheckToggle(paused);

        CursorItem.TryRefresh(paused || !IsEditing);

        if (!IsEditing) return;

        HeroController.instance.GetComponent<Rigidbody2D>().velocity.Set(0, 0);

        // Checks if the selected item is placeable
        if (EditorUIManager.SelectedItem is PlaceableObject placeable && !paused)
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

                    if (Input.GetKey(KeyCode.LeftShift)) i = -i;
                    
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

        if (Architect.GlobalSettings.Keybinds.AddPrefab.WasPressed) PrefabsCategory.TryAddPrefab();

        var actions = InputHandler.Instance.inputActions;
        
        if (paused) ShiftSelection(actions);
        else TryPlace();

        if (_prevPaused != paused)
        {
            _prevPaused = paused;
            foreach (var element in EditorUIManager.PauseOptions)
            {
                element.Visibility = paused ? Visibility.Visible : Visibility.Hidden;
            }
        }

        DoFreeMovement(actions, paused);
    }

    private static Vector3 _freeMovePos;

    private static void DoFreeMovement(HeroActions actions, bool paused)
    {
        var up = actions.up.IsPressed;
        var down = actions.down.IsPressed;
        var left = actions.left.IsPressed;
        var right = actions.right.IsPressed;

        if (!paused && up != down)
        {
            _freeMovePos += (up ? Vector3.up : Vector3.down) * Time.deltaTime * 20;
        }

        if (!paused && left != right)
        {
            _freeMovePos += (left ? Vector3.left : Vector3.right) * Time.deltaTime * 20;
        }

        if (HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_TRANSITION)
        {
            HeroController.instance.transform.position = _freeMovePos;
        }   
        else _freeMovePos = HeroController.instance.transform.position;
    }

    private static void CheckToggle(bool paused)
    {
        if (paused) return;
        _prevPaused = true;
        
        if (!Architect.GlobalSettings.Keybinds.ToggleEditor.WasPressed) return;
        var fsm = HeroController.instance.gameObject.LocateMyFSM("Surface Water");
        if (fsm.ActiveStateName == "Idle") fsm.SetState("Regain Control");
        
        if (Dragged.Count > 0) ReleaseDraggedItems(true);

        if (LostControlToCustomObject)
        {
            LostControlToCustomObject = false;
            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();
        }
        
        if (HeroController.instance.controlReqlinquished) return;

        if (IsEditing) SceneSaveLoader.SaveScene(GameManager.instance.sceneName, PlacementManager.GetCurrentPlacements());
        else _freeMovePos = HeroController.instance.transform.position;

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
        if (leftMenu != rightMenu) EditorUIManager.ShiftGroup(rightMenu ? 1 : -1);
    }

    private static void TryPlace()
    {
        if (Architect.GlobalSettings.Keybinds.Undo.WasPressed) UndoManager.UndoLast();
        if (Architect.GlobalSettings.Keybinds.Redo.WasPressed) UndoManager.RedoLast();
        
        var b1 = Input.GetMouseButtonDown(0);
        var b2 = Input.GetMouseButton(0);
        
        if (!b2) ResetObject.RestartDelay();

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

    public static readonly List<DraggedObject> Dragged = [];
    private static bool _dragging;

    public static void AddDraggedItem(ObjectPlacement placement, Vector3 clickPos, bool startDragging)
    {
        if (placement.StartDragging(!startDragging))
        {
            placement.StoreOldPos();
            Dragged.Add(new DraggedObject(placement, placement.GetPos() - clickPos));
        } else if (!startDragging) Dragged.RemoveAll(obj => obj.Placement == placement);
        
        if (startDragging) BeginDragging(clickPos);
    }

    public static void BeginDragging(Vector3 clickPos)
    {
        foreach (var dragged in Dragged) dragged.Offset = dragged.Placement.GetPos() - clickPos;
        _dragging = true;
    }

    public static void ReleaseDraggedItems(bool storeUndo)
    {
        var deselect = !Input.GetKey(KeyCode.LeftControl);
        
        if (storeUndo) UndoManager.PerformAction(new MoveObjects(
            Dragged.Select(obj => (obj.Placement.GetId(), obj.Placement.GetOldPos())).ToList()
        ));
        
        foreach (var dragged in Dragged)
        {
            if (deselect) dragged.Placement.StopDragging();
            var id = dragged.Placement.GetId();
            if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
            {
                HkmpHook.Update(id, GameManager.instance.sceneName, dragged.Placement.GetPos());
            }

            dragged.Placement.StoreOldPos();
        }
        
        _dragging = false;
        if (deselect) Dragged.Clear();
    }

    private static bool _groupSelecting;
    private static Vector3 _groupSelectionCorner;
    private static GroupSelectionBox _groupSelectionBox;

    public static void StartGroupSelect(Vector3 pos)
    {
        if (Dragged.Count > 0) return;
        
        _groupSelectionCorner = pos;
        _groupSelecting = true;
        
        _groupSelectionBox.gameObject.SetActive(true);
        _groupSelectionBox.transform.position = pos;
        _groupSelectionBox.width = 0;
        _groupSelectionBox.height = 0;
        _groupSelectionBox.UpdateOutline();
    }

    private static readonly List<DraggedObject> CopiedObjects = [];

    public static void DoCopy()
    {
        CopiedObjects.Clear();
        var wp = GetWorldPos(Input.mousePosition);

        CopiedObjects.AddRange(Dragged.Select(obj => 
            new DraggedObject(obj.Placement, wp - obj.Placement.GetPos())));
    }

    public static void DoPaste()
    {
        ReleaseDraggedItems(false);
        var wp = GetWorldPos(Input.mousePosition);

        var converts = new Dictionary<string, string>();
        foreach (var obj in CopiedObjects)
        {
            converts[obj.Placement.GetId()] = Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        foreach (var obj in CopiedObjects)
        {
            var pl = obj.Placement;

            var updatedConfig = new List<ConfigValue>();

            foreach (var conf in pl.Config)
            {
                if (conf is StringConfigValue value)
                {
                    var val = value.GetValue();
                    updatedConfig.Add(converts.TryGetValue(val, out var convert)
                        ? Attributes.ConfigManager.DeserializeConfigValue(conf.GetTypeId(), convert)
                        : conf);
                } else updatedConfig.Add(conf);
            }
            
            var np = new ObjectPlacement(
                pl.Name,
                wp - obj.Offset,
                pl.Flipped,
                pl.Rotation,
                pl.Scale,
                converts[pl.GetId()],
                pl.Broadcasters,
                pl.Receivers,
                updatedConfig.ToArray()
            );
            PlacementManager.GetCurrentPlacements().Add(np);
            np.PlaceGhost();
            AddDraggedItem(np, wp, false);

            if (Architect.UsingMultiplayer && Architect.GlobalSettings.CollaborationMode)
            {
                HkmpHook.Place(np, GameManager.instance.sceneName);
            }
        }
        
        UndoManager.PerformAction(new PlaceObject(converts.Values.ToList()));
    }

    public static void StopGroupSelect(Vector3 pos)
    {
        _groupSelectionBox.gameObject.SetActive(false);

        foreach (var obj in PlacementManager.GetCurrentPlacements()
                     .Where(obj => obj.IsWithinZone(_groupSelectionCorner, pos)))
        {
            AddDraggedItem(obj, pos, false);
        }

        _groupSelecting = false;
    }
    
    private static void RefreshGroupSelectionBox()
    {
        if (!_groupSelecting) return;

        var wp = GetWorldPos(Input.mousePosition);
        if (Input.GetMouseButtonUp(0))
        {
            StopGroupSelect(wp);
            return;
        }
        
        _groupSelectionBox.width = wp.x - _groupSelectionCorner.x;
        _groupSelectionBox.height = wp.y - _groupSelectionCorner.y;
        _groupSelectionBox.UpdateOutline();
    }

    private static readonly int Color1 = Shader.PropertyToID("_Color");
    
    private static void SetupGroupSelectionBox()
    {
        var box = new GameObject("[Architect] Group Selection Box");
        Object.DontDestroyOnLoad(box);
        box.SetActive(false);

        var particleMaterial = new Material(Shader.Find("Sprites/Default"));
        particleMaterial.SetColor(Color1, new Color(0, 1, 0, 0.3f));
        box.AddComponent<LineRenderer>().material = particleMaterial;
        
        _groupSelectionBox = box.AddComponent<GroupSelectionBox>();
    }

    public class DraggedObject(ObjectPlacement placement, Vector3 offset)
    {
        public readonly ObjectPlacement Placement = placement;
        public Vector3 Offset = offset;
    }
}