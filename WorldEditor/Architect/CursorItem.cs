using Architect.Attributes.Config;
using UnityEngine;
using Architect.Objects;
using Architect.UI;
using Architect.Util;

namespace Architect;

public static class CursorItem
{
    public static bool NeedsRefreshing;

    private static GameObject _obj;

    private static Vector3 _offset;
    
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

        if (!NeedsRefreshing) return;
        
        NeedsRefreshing = false;

        var renderer = _obj.GetComponent<SpriteRenderer>();
        var scaleX = EditorManager.Scale;
        var scaleY = EditorManager.Scale;

        if (EditorUIManager.ConfigValues.TryGetValue("width", out var widthVal) && widthVal is FloatConfigValue width) scaleX *= width.GetValue();
        if (EditorUIManager.ConfigValues.TryGetValue("height", out var heightVal) && heightVal is FloatConfigValue height) scaleY *= height.GetValue();
        if (EditorUIManager.ConfigValues.TryGetValue("layer", out var layerVal) && layerVal is IntConfigValue layer)
            renderer.sortingOrder = layer.GetValue();
        
        _offset = ResourceUtils.FixOffset(selected.Offset, EditorManager.IsFlipped, EditorManager.Rotation, EditorManager.Scale);
        GhostPlacementUtils.SetupForPlacement(_obj, renderer, selected, EditorManager.IsFlipped, EditorManager.Rotation, scaleX, scaleY);
    }

    private static void SetupObject()
    {
        NeedsRefreshing = true;
        _obj = new GameObject { name = "[Architect] Cursor Preview" };
        _obj.AddComponent<SpriteRenderer>().color = new Color(1, 0.2f, 0.2f, 0.5f);
        Object.DontDestroyOnLoad(_obj);
    }
}