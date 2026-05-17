using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Modding;

namespace Architect.UI;

[Serializable]
public class MenuTransformData
{
    public string Name;
    public float X;
    public float Y;
}

[Serializable]
public class MenuLayoutData
{
    public List<MenuTransformData> Elements = new();
}

public static class EditMenuManager
{
    private static bool _editMode;

    private static readonly Dictionary<string, RectTransform> _menuObjects = new();
    private static RectTransform _selected;

    private static Canvas _canvas;
    private static RectTransform _canvasRect;
    private static UnityEngine.UI.Button _editButton;

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "HKMenuLayout.json");

    // =========================================================
public static void Initialize(object _)
{
    On.UIManager.Start += UIManager_Start;
    On.UIManager.UIGoToMainMenu += UIManager_UIGoToMainMenu;

    var host = new GameObject("HKMenuEditor");
    UnityEngine.Object.DontDestroyOnLoad(host);
    host.AddComponent<Updater>();
}

private static IEnumerator SetupAfterMenu(UIManager self)
{
    yield return null;
    SetupCanvas();
    CacheMenu();
    LoadLayout();
}

    // =========================================================
    private static void UIManager_Start(On.UIManager.orig_Start orig, UIManager self)
{
    orig(self);
    self.StartCoroutine(SetupAfterMenu(self));
}

    private static void UIManager_UIGoToMainMenu(On.UIManager.orig_UIGoToMainMenu orig, UIManager self)
{
    orig(self);
    self.StartCoroutine(SetupAfterMenu(self));
}

    // =========================================================
    private static void SetupCanvas()
    {
        var canvasGO = GameObject.Find("_UIManager/UICanvas");
        if (canvasGO == null)
        {
            Debug.LogError("[EditMenu] Canvas not found");
            return;
        }

        _canvas = canvasGO.GetComponent<Canvas>();
        _canvasRect = canvasGO.GetComponent<RectTransform>();
    }

    // =========================================================
    private static void CacheMenu()
{
    _menuObjects.Clear();

    var root = GameObject.Find("_UIManager/UICanvas/MainMenuScreen/MainMenuButtons");
    if (root == null)
    {
        Debug.LogError("[EditMenu] MainMenuButtons not found");
        return;
    }

    foreach (Transform child in root.transform)
    {
        var rt = child.GetComponent<RectTransform>(); // ← fix here
        if (rt != null)
        {
            _menuObjects[child.name] = rt;
            Debug.Log("[EditMenu] Cached: " + child.name);
        }
    }
}

    // =========================================================
    private static void CreateEditButton()
{
    var parent = GameObject.Find("_UIManager/UICanvas/MainMenuScreen");
    if (parent == null)
    {
        Debug.LogError("[EditMenu] MainMenuScreen not found");
        return;
    }

    // If button already exists in scene, reuse it
    var existing = parent.transform.Find("EditMenuButton");
    if (existing != null)
    {
        _editButton = existing.GetComponent<UnityEngine.UI.Button>();
        return;
    }

    var go = new GameObject("EditMenuButton");
    go.transform.SetParent(parent.transform, false);

    var rect = go.AddComponent<RectTransform>();
    rect.anchorMin = new Vector2(1, 1);
    rect.anchorMax = new Vector2(1, 1);
    rect.pivot = new Vector2(1, 1);
    rect.anchoredPosition = new Vector2(-200, -120); // adjust if needed
    rect.sizeDelta = new Vector2(160, 60);

    var img = go.AddComponent<UnityEngine.UI.Image>();
    img.color = new Color(0, 0, 0, 0.8f);

    _editButton = go.AddComponent<UnityEngine.UI.Button>();
    _editButton.targetGraphic = img;

    // TEXT
    var textGO = new GameObject("Text");
    textGO.transform.SetParent(go.transform, false);

    var textRect = textGO.AddComponent<RectTransform>();
    textRect.anchorMin = Vector2.zero;
    textRect.anchorMax = Vector2.one;
    textRect.offsetMin = Vector2.zero;
    textRect.offsetMax = Vector2.zero;

    var text = textGO.AddComponent<Text>();
    text.text = "EDIT";
    text.alignment = TextAnchor.MiddleCenter;
    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    text.color = Color.white;

    _editButton.onClick.AddListener(() =>
    {
        _editMode = !_editMode;
        Debug.Log("[EditMenu] Edit mode: " + _editMode);

        if (!_editMode)
            _selected = null;
    });

    Debug.Log("[EditMenu] Button created");
}

    // =========================================================
    private static IEnumerator ApplyNextFrame()
    {
        yield return null;
        LoadLayout();
    }

    // =========================================================
    public static void Update()
{
    var sceneName = GameManager.instance?.sceneName;
    if (sceneName != "Menu_Title") return;

    if (Input.GetKeyDown(KeyCode.M))
    {
        _editMode = !_editMode;
        if (!_editMode)
        {
            SaveLayout();
            _selected = null;
        }
        Debug.Log("[EditMenu] Edit mode: " + _editMode);
    }

    if (!_editMode || _canvasRect == null) return;

    Vector2 localPoint;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        _canvasRect,
        Input.mousePosition,
        null,
        out localPoint
    );

    if (Input.GetMouseButtonDown(0))
    {
        foreach (var kv in _menuObjects)
        {
            var rt = kv.Value.GetComponent<RectTransform>();
            if (rt != null && Vector2.Distance(rt.anchoredPosition, localPoint) < 100f)
            {
                _selected = rt;
                Debug.Log("[EditMenu] Selected: " + kv.Key);
                break;
            }
        }
    }

    if (_selected != null && Input.GetMouseButton(0))
    {
        _selected.anchoredPosition = localPoint;
    }

    if (Input.GetKeyDown(KeyCode.S))
    {
        SaveLayout();
    }
}

    // =========================================================
    private static void SaveLayout()
    {
        var data = new MenuLayoutData();

        foreach (var kv in _menuObjects)
        {
            var t = kv.Value;
            data.Elements.Add(new MenuTransformData
            {
                Name = kv.Key,
                X = t.anchoredPosition.x,
                Y = t.anchoredPosition.y
            });
        }

        File.WriteAllText(SavePath,
            JsonConvert.SerializeObject(data, Formatting.Indented));

        Debug.Log("[EditMenu] Layout saved");
    }

    private static void LoadLayout()
    {
        if (!File.Exists(SavePath)) return;

        var data = JsonConvert.DeserializeObject<MenuLayoutData>(
            File.ReadAllText(SavePath));

        foreach (var e in data.Elements)
        {
            if (_menuObjects.TryGetValue(e.Name, out var t))
            {
                t.anchoredPosition = new Vector2(e.X, e.Y);
            }
        }

        Debug.Log("[EditMenu] Layout loaded");
    }

    // =========================================================
    private class Updater : MonoBehaviour
    {
        void Update() => EditMenuManager.Update();
    }
}