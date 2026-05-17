using UnityEngine;
using Architect.Content.Elements.Custom.Behaviour;

namespace Architect.Content.Elements.Custom.Behaviour;

public class CustomTalkUI : MonoBehaviour
{
    public static CustomTalkUI Instance;

    private bool isOpen = false;
    private string inputText = "";
    private CustomTalk targetNPC = null;

    // Panel dimensions
    private const float PanelWidth = 500f;
    private const float PanelHeight = 300f;

    private Texture2D panelTexture;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private GUIStyle panelStyle;
    private GUIStyle textAreaStyle;
    private GUIStyle buttonStyle;
    private GUIStyle titleStyle;
    private bool stylesInitialized = false;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Open(CustomTalk npc)
    {
        targetNPC = npc;
        // Pre-fill with existing dialogue joined by | for editing
        inputText = npc.dialoguePages != null
            ? string.Join("|", npc.dialoguePages)
            : "";
        isOpen = true;
        // Pause game while typing
        Time.timeScale = 0f;
    }

    public void Close()
    {
        isOpen = false;
        targetNPC = null;
        Time.timeScale = 1f;
    }

    void InitStyles()
    {
        if (stylesInitialized) return;

        // Rounded panel background
        panelTexture = MakeRoundedTexture(
            (int)PanelWidth, (int)PanelHeight,
            new Color(0.08f, 0.08f, 0.12f, 0.95f), // dark bg
            new Color(0.4f, 0.4f, 0.6f, 1f),        // border
            20, 3);

        // Save button textures
        buttonTexture = MakeRoundedTexture(
            120, 40,
            new Color(0.2f, 0.5f, 0.8f, 1f),
            new Color(0.4f, 0.7f, 1f, 1f),
            10, 2);
        buttonHoverTexture = MakeRoundedTexture(
            120, 40,
            new Color(0.3f, 0.6f, 0.9f, 1f),
            new Color(0.5f, 0.8f, 1f, 1f),
            10, 2);

        panelStyle = new GUIStyle
        {
            normal = { background = panelTexture },
            padding = new RectOffset(20, 20, 20, 20)
        };

        textAreaStyle = new GUIStyle(GUI.skin.textArea)
        {
            fontSize = 16,
            wordWrap = true,
            normal = { 
                textColor = Color.white,
                background = MakePlainTexture(new Color(0.15f, 0.15f, 0.2f, 1f))
            },
            focused = {
                textColor = Color.white,
                background = MakePlainTexture(new Color(0.18f, 0.18f, 0.25f, 1f))
            },
            padding = new RectOffset(10, 10, 10, 10)
        };

        buttonStyle = new GUIStyle
        {
            normal = { 
                background = buttonTexture,
                textColor = Color.white
            },
            hover = {
                background = buttonHoverTexture,
                textColor = Color.white
            },
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        titleStyle = new GUIStyle
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.7f, 0.85f, 1f) },
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(0, 0, 0, 10)
        };

        stylesInitialized = true;
    }

    void OnGUI()
    {
        if (!isOpen) return;
        InitStyles();

        float x = (Screen.width - PanelWidth) / 2f;
        float y = (Screen.height - PanelHeight) / 2f;
        var panelRect = new Rect(x, y, PanelWidth, PanelHeight);

        // Draw panel
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(new Rect(x + 24, y + 24, PanelWidth - 48, PanelHeight - 48));

        // Title
        GUILayout.Label("NPC Dialogue", titleStyle);

        // Hint
        GUI.color = new Color(0.6f, 0.6f, 0.7f);
        GUILayout.Label("Use | to separate pages   e.g.  Hello!|Farewell, traveller.", GUI.skin.label);
        GUI.color = Color.white;

        GUILayout.Space(8);

        // Text input — takes up remaining space minus button row
        inputText = GUILayout.TextArea(inputText, textAreaStyle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(PanelHeight - 160));

        GUILayout.Space(10);

        // Button row — save on right, cancel on left
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Cancel", GUILayout.Width(100), GUILayout.Height(38)))
            Close();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save", buttonStyle, GUILayout.Width(120), GUILayout.Height(38)))
            Save();

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void Save()
    {
        if (targetNPC == null) { Close(); return; }
        targetNPC.dialoguePages = inputText.Split('|');
        Close();
    }

    // --- Texture helpers ---

    Texture2D MakeRoundedTexture(int w, int h, Color fill, Color border, int radius, int borderWidth)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];

        for (int py = 0; py < h; py++)
        for (int px = 0; px < w; px++)
        {
            bool inside = IsInsideRounded(px, py, w, h, radius);
            bool onBorder = inside && !IsInsideRounded(
                px, py, w, h, radius - borderWidth,
                borderWidth, borderWidth, borderWidth, borderWidth);
            pixels[py * w + px] = !inside ? Color.clear
                                 : onBorder ? border
                                 : fill;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    bool IsInsideRounded(int px, int py, int w, int h, int r,
        int offX = 0, int offY = 0, int offW = 0, int offH = 0)
    {
        int x0 = offX + r, y0 = offY + r;
        int x1 = w - offW - r, y1 = h - offH - r;
        if (px >= x0 && px <= x1 && py >= y0 && py <= y1) return true;
        if (px >= x0 && px <= x1 && py >= offY && py < y0) return true;
        if (px >= x0 && px <= x1 && py > y1 && py <= h - offH - 1) return true;
        if (py >= y0 && py <= y1 && px >= offX && px < x0) return true;
        if (py >= y0 && py <= y1 && px > x1 && px <= w - offW - 1) return true;
        // Corners
        float dx, dy;
        dx = px - x0; dy = py - y0; if (px < x0 && py < y0 && dx*dx+dy*dy > r*r) return false;
        dx = px - x1; dy = py - y0; if (px > x1 && py < y0 && dx*dx+dy*dy > r*r) return false;
        dx = px - x0; dy = py - y1; if (px < x0 && py > y1 && dx*dx+dy*dy > r*r) return false;
        dx = px - x1; dy = py - y1; if (px > x1 && py > y1 && dx*dx+dy*dy > r*r) return false;
        return px >= offX && px <= w-offW-1 && py >= offY && py <= h-offH-1;
    }

    Texture2D MakePlainTexture(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }
}