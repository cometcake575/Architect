using Architect.Attributes.Config;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Content.Groups;
using UnityEngine;
using Architect.Objects;
using Architect.UI;
using Architect.Util;

namespace Architect;

public static class CursorItem
{
    public static bool NeedsRefreshing;

    private static GameObject _obj;
    private static GameObject _display;

    private static Vector3 _offset;

    private static MovingObject _movingComponent;

    private static bool _previewMode;
    
    public static void TryRefresh(bool disabled)
    {
        if (disabled || EditorUIManager.SelectedItem is not PlaceableObject selected || !EditorManager.GameCamera)
        {
            if (_obj) _obj.SetActive(false);
            return;
        }
        
        if (!_obj) SetupObject();
        else _obj.SetActive(true);

        var pos = Input.mousePosition;
        pos.z = -EditorManager.GameCamera.transform.position.z;

        var objPos = EditorManager.GetWorldPos(pos);
        
        objPos.z = selected.GetZPos();

        _obj.transform.position = objPos + _offset;

        if (NeedsRefreshing) _previewMode = false;
        if (Architect.GlobalSettings.Keybinds.TogglePreview.WasPressed)
        {
            _previewMode = true;
            NeedsRefreshing = true;
        } else if (Architect.GlobalSettings.Keybinds.TogglePreview.WasReleased)
        {
            _previewMode = false;
            NeedsRefreshing = true;
        }

        if (!NeedsRefreshing) return;
        
        NeedsRefreshing = false;

        var renderer = _display.GetComponent<SpriteRenderer>();
        var scaleX = EditorManager.Scale;
        var scaleY = EditorManager.Scale;

        ConfigValue cfgVal;
        if (EditorUIManager.ConfigValues.TryGetValue("width", out cfgVal) && cfgVal is FloatConfigValue width) scaleX *= width.GetValue();
        if (EditorUIManager.ConfigValues.TryGetValue("height", out cfgVal) && cfgVal is FloatConfigValue height) scaleY *= height.GetValue();
        
        if (EditorUIManager.ConfigValues.TryGetValue("layer", out cfgVal) && cfgVal is IntConfigValue layer) renderer.sortingOrder = layer.GetValue();
        else renderer.sortingOrder = 0;

        UpdateMovingComponent();
        
        _offset = ResourceUtils.FixOffset(selected.Offset, EditorManager.IsFlipped, EditorManager.Rotation, EditorManager.Scale);
        GhostPlacementUtils.SetupForPlacement(_display, renderer, selected, EditorManager.IsFlipped, EditorManager.Rotation, scaleX, scaleY);
    }

    private static void SetupObject()
    {
        NeedsRefreshing = true;
        _obj = new GameObject { name = "[Architect] Cursor Preview" };
        _display = new GameObject("Cursor Display") { transform = { parent = _obj.transform } };
        
        _display.AddComponent<SpriteRenderer>().color = new Color(1, 0.2f, 0.2f, 0.5f);
        _movingComponent = _display.AddComponent<MovingObject>();
        _movingComponent.SetSpeed(0);
        
        Object.DontDestroyOnLoad(_obj);
    }

    private static void UpdateMovingComponent()
    {
        ConfigValue cfgVal;
        
        _movingComponent.PreviewReset();
        
        if (!_previewMode)
        {
            _movingComponent.SetSpeed(0);
            return;
        }
        
        var movable = false;
        
        if (EditorUIManager.ConfigValues.TryGetValue("Track Distance", out cfgVal) && cfgVal is FloatConfigValue dist)
        {
            _movingComponent.trackDistance = dist.GetValue();
            movable = true;
        }
        if (EditorUIManager.ConfigValues.TryGetValue("Speed", out cfgVal) && cfgVal is FloatConfigValue speed)
        {
            _movingComponent.SetSpeed(speed.GetValue());
            movable = true;
        }
        if (EditorUIManager.ConfigValues.TryGetValue("Pause Time", out cfgVal) && cfgVal is FloatConfigValue pt)
        {
            _movingComponent.pauseTime = pt.GetValue();
            movable = true;
        }
        if (EditorUIManager.ConfigValues.TryGetValue("Smoothing", out cfgVal) && cfgVal is FloatConfigValue smooth)
        {
            _movingComponent.smoothing = smooth.GetValue();
            movable = true;
        }
        if (EditorUIManager.ConfigValues.TryGetValue("Start Offset", out cfgVal) && cfgVal is FloatConfigValue so)
        {
            _movingComponent.offset = so.GetValue();
            movable = true;
        }
        if (EditorUIManager.ConfigValues.TryGetValue("Track Rotation", out cfgVal) && cfgVal is FloatConfigValue tr)
        {
            _movingComponent.rotation = tr.GetValue();
            movable = true;
        }
        if (EditorUIManager.ConfigValues.TryGetValue("Rotation over Time", out cfgVal) && cfgVal is FloatConfigValue rot)
        {
            _movingComponent.rotationOverTime = rot.GetValue();
            movable = true;
        }

        if (!movable) _movingComponent.SetSpeed(0);
    }
}