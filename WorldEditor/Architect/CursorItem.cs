using Architect.Attributes.Config;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.Objects;
using Architect.Storage;
using Architect.UI;
using Architect.Util;
using UnityEngine;
using UnityEngine.Video;

namespace Architect;

public static class CursorItem
{
    private static VideoPlayer _player;
    public static bool NeedsRefreshing;

    private static GameObject _obj;
    private static GameObject _display;
    private static LocalTrailRenderer _trail;

    private static Vector3 _offset;

    private static MovingObject _movingComponent;

    private static bool _previewMode;
    private static readonly int Color1 = Shader.PropertyToID("_Color");

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
        }
        else if (Architect.GlobalSettings.Keybinds.TogglePreview.WasReleased)
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
        if (EditorUIManager.ConfigValues.TryGetValue("width", out cfgVal) && cfgVal is FloatConfigValue width)
            scaleX *= width.GetValue();
        if (EditorUIManager.ConfigValues.TryGetValue("height", out cfgVal) && cfgVal is FloatConfigValue height)
            scaleY *= height.GetValue();

        string newSprite = null;
        var point = false;
        var ppu = 100f;
        if (EditorUIManager.ConfigValues.TryGetValue("Source URL", out cfgVal) && cfgVal is StringConfigValue source)
            newSprite = source.GetValue();
        if (EditorUIManager.ConfigValues.TryGetValue("Clip URL", out cfgVal) && cfgVal is StringConfigValue clip)
            CustomAssetLoader.PrepareClip(clip.GetValue());
        if (EditorUIManager.ConfigValues.TryGetValue("Video URL", out cfgVal) && cfgVal is StringConfigValue video)
        {
            _player.enabled = true;
            CustomAssetLoader.DoLoadVideo(_display, null, video.GetValue());
        }
        else
        {
            _player.enabled = false;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Filter", out cfgVal) && cfgVal is ChoiceConfigValue filter)
            point = filter.GetValue() == 0;
        if (EditorUIManager.ConfigValues.TryGetValue("Pixels Per Unit", out cfgVal) &&
            cfgVal is FloatConfigValue ppuVal) ppu = ppuVal.GetValue();

        if (EditorUIManager.ConfigValues.TryGetValue("layer", out cfgVal) && cfgVal is IntConfigValue layer)
            renderer.sortingOrder = layer.GetValue();
        else renderer.sortingOrder = 0;

        UpdateMovingComponent();

        _offset = ResourceUtils.FixOffset(selected.Offset, EditorManager.IsFlipped, EditorManager.Rotation,
            EditorManager.Scale);
        if (EditorUIManager.ConfigValues.TryGetValue("Z Offset", out cfgVal) && cfgVal is FloatConfigValue zOffset)
            _offset.z += zOffset.GetValue();

        GhostPlacementUtils.SetupForPlacement(_display, renderer, selected, EditorManager.IsFlipped,
            EditorManager.Rotation, scaleX, scaleY);

        if (newSprite != null) CustomAssetLoader.DoLoadSprite(_display, newSprite, point, ppu);
    }

    private static void SetupObject()
    {
        NeedsRefreshing = true;
        _obj = new GameObject("[Architect] Cursor Preview");
        _display = new GameObject("Cursor Display") { transform = { parent = _obj.transform } };

        _display.AddComponent<SpriteRenderer>().color = new Color(1, 0.2f, 0.2f, 0.5f);
        _movingComponent = _display.AddComponent<MovingObject>();
        _movingComponent.SetSpeed(0);

        _player = _display.AddComponent<VideoPlayer>();
        _player.playbackSpeed = 0;

        _trail = _display.AddComponent<LocalTrailRenderer>();
        var line = _obj.AddComponent<LineRenderer>();
        _trail.lineRenderer = line;

        line.startWidth = 0.1f;
        line.startColor = Color.red;

        var particleMaterial = new Material(Shader.Find("Sprites/Default"));
        particleMaterial.SetColor(Color1, new Color(1, 0, 0, 0.2f));
        line.material = particleMaterial;

        Object.DontDestroyOnLoad(_obj);
    }

    private static void UpdateMovingComponent()
    {
        ConfigValue cfgVal;

        _trail.Reset();
        _movingComponent.PreviewReset();

        if (!_previewMode)
        {
            _movingComponent.SetSpeed(0);
            _trail.enabled = false;
            return;
        }

        var movable = false;

        if (EditorUIManager.ConfigValues.TryGetValue("Track Distance", out cfgVal) && cfgVal is FloatConfigValue dist)
        {
            _movingComponent.trackDistance = dist.GetValue();
            movable = true;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Track Movement Speed", out cfgVal) &&
            cfgVal is FloatConfigValue speed)
        {
            _movingComponent.SetSpeed(speed.GetValue());
            movable = true;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Track Pause Time", out cfgVal) && cfgVal is FloatConfigValue pt)
        {
            _movingComponent.pauseTime = pt.GetValue();
            movable = true;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Track Smoothing", out cfgVal) &&
            cfgVal is FloatConfigValue smooth)
        {
            _movingComponent.smoothing = smooth.GetValue();
            movable = true;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Start Offset on Track", out cfgVal) &&
            cfgVal is FloatConfigValue so)
        {
            _movingComponent.offset = so.GetValue();
            movable = true;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Track Rotation", out cfgVal) && cfgVal is FloatConfigValue tr)
        {
            _movingComponent.rotation = tr.GetValue();
            movable = true;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Rotation over Time", out cfgVal) &&
            cfgVal is FloatConfigValue rot)
        {
            _movingComponent.SetRotationSpeed(rot.GetValue());
            movable = true;
        }

        if (EditorUIManager.ConfigValues.TryGetValue("Stick Player", out cfgVal)) movable = true;

        if (!movable)
        {
            _movingComponent.SetSpeed(0);
            _trail.enabled = false;
        }
        else
        {
            _trail.enabled = true;
        }
    }
}