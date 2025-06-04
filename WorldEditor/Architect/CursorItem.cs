using UnityEngine;
using Architect.objects;
using Architect.UI;

namespace Architect;

public static class CursorItem
{
    public static bool NeedsRefreshing;

    private static GameObject _obj;
    
    public static void TryRefresh(bool disabled)
    {
        if (!Architect.Target || disabled || EditorUIManager.SelectedItem is not PlaceableObject selected)
        {
            if (_obj) _obj.SetActive(false);
            return;
        }
        
        if (!_obj) SetupObject();
        else _obj.SetActive(true);

        Vector3 pos = Input.mousePosition;
        pos.z = -Architect.Target.transform.position.z;
        _obj.transform.position = Architect.Target.ScreenToWorldPoint(pos);

        if (!NeedsRefreshing) return;

        NeedsRefreshing = false;

        SpriteRenderer renderer = _obj.GetComponent<SpriteRenderer>();
        renderer.sprite = selected.GetSprite();
        _obj.transform.localScale = selected.GetPrefab().transform.localScale;
        int rotation = selected.GetRotation() + Architect.Rotation;
        _obj.transform.rotation = Quaternion.Euler(0, 0, rotation);
        if (rotation % 180 != 0)
        {
            renderer.flipY = Architect.IsFlipped;
            renderer.flipX = selected.FlipX();
        }
        else
        {
            renderer.flipX = Architect.IsFlipped;
            renderer.flipY = selected.FlipX();
        }
    }

    private static void SetupObject()
    {
        NeedsRefreshing = true;
        _obj = new GameObject();
        _obj.AddComponent<SpriteRenderer>().color = new Color(1, 0.2f, 0.2f, 0.5f);
        Object.DontDestroyOnLoad(_obj);
    }
}