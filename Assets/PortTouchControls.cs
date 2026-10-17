// Touch controls for the Android port.
// Put this file at Assets/PortTouchControls.cs. It creates itself at startup.
// It feeds a virtual keyboard into the Input System, so the game thinks keys are pressed.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PortTouchControls : MonoBehaviour
{
    const string PrefsKey = "PortTouchLayoutV1";
    static readonly string[] HiddenScenes = { "IntroScene", "MainMenu" };
    const float RefW = 2400f, RefH = 1080f;      // layout below is designed on a 2400x1080 screen
    const float DefaultAlpha = 0.65f;
    const float MinScale = 0.5f, MaxScale = 2.0f, MinAlpha = 0.2f, MaxAlpha = 1.0f;

    // ------------------------------------------------------------------ data
    public class Ctl
    {
        public string id, label;
        public Key key;
        public bool dpad;
        public Vector2 defPx;      // default centre in reference pixels
        public float size;         // diameter in canvas units at scale 1
        public Vector2 pos;        // centre as a fraction of the screen
        public float scale = 1f, alpha = DefaultAlpha;
        public RectTransform rt;
        public Image glow, bg, ring;
        public Image[] arrows;
        public CanvasGroup cg;
        public bool down;
        public int touchId = -1;
        public bool[] dir = new bool[4];   // up, down, left, right
    }

    [Serializable] public class SaveItem { public string id; public float x, y, scale, alpha; }
    [Serializable] public class SaveData { public List<SaveItem> items = new List<SaveItem>(); }

    class Cap { public int kind; public Ctl ctl; public int slider; public Vector2 grab; }   // 0 ignore, 1 press, 2 drag, 3 slider

    enum Mode { Play, Popup, Edit }

    // ------------------------------------------------------------------ state
    Canvas canvas;
    RectTransform controlsRoot, uiRoot;
    Font font;
    readonly List<Ctl> ctls = new List<Ctl>();
    Keyboard kb;
    readonly HashSet<Key> sent = new HashSet<Key>();
    readonly HashSet<Key> want = new HashSet<Key>();
    Mode mode = Mode.Play;
    bool optionsOpen, visible;
    Ctl selected;
    Vector4[] snapshot;

    Sprite sprCircle, sprRing, sprRRect, sprArrow, sprGear, sprX, sprNotch;
    RectTransform notchRT, popupRT, slidersRT, optionsRT;
    Image notchIcon;
    readonly RectTransform[] optBtn = new RectTransform[3];
    readonly RectTransform[] sliderRow = new RectTransform[2];
    readonly RectTransform[] sliderTrack = new RectTransform[2];
    readonly RectTransform[] sliderFill = new RectTransform[2];
    readonly RectTransform[] sliderKnob = new RectTransform[2];
    readonly Text[] sliderVal = new Text[2];

    readonly Dictionary<int, Cap> caps = new Dictionary<int, Cap>();
    readonly HashSet<int> seen = new HashSet<int>();
    readonly List<int> gone = new List<int>();
    readonly Vector3[] corners = new Vector3[4];

    static readonly Color Pink = new Color(1f, 0.33f, 0.58f, 1f);

    // ------------------------------------------------------------------ startup
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var go = new GameObject("PortTouchControls");
        DontDestroyOnLoad(go);
        go.AddComponent<PortTouchControls>();
    }

    void Awake()
    {
        try { EnhancedTouchSupport.Enable(); } catch (Exception e) { Debug.LogError("Touch enable failed: " + e.Message); }
        try { kb = InputSystem.AddDevice<Keyboard>("PortTouchKeyboard"); } catch (Exception e) { Debug.LogError("Virtual keyboard failed: " + e.Message); }
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Font.CreateDynamicFontFromOSFont("sans-serif", 16);
        BuildSprites();
        BuildDefaults();
        LoadLayout();
        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyScene(SceneManager.GetActiveScene().name);
        FixUiModules();
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        ApplyScene(s.name);
        FixUiModules();
    }

    void ApplyScene(string sceneName)
    {
        if (mode == Mode.Edit) { RestoreSnapshot(); ExitEdit(); }
        mode = Mode.Play;
        visible = Array.IndexOf(HiddenScenes, sceneName) < 0;
        if (canvas != null) canvas.gameObject.SetActive(visible);
        if (!visible) ReleaseAll();
    }

    // Tapping our buttons must not clear the game's selected menu item
    void FixUiModules()
    {
        var mods = FindObjectsOfType<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        for (int i = 0; i < mods.Length; i++)
        {
            var p = mods[i].GetType().GetProperty("deselectOnBackgroundClick");
            if (p != null && p.CanWrite) p.SetValue(mods[i], false, null);
        }
    }

    // ------------------------------------------------------------------ layout data
    void Add(string id, string label, Key key, float px, float py, float size, bool dpad = false)
    {
        var c = new Ctl();
        c.id = id; c.label = label; c.key = key; c.dpad = dpad;
        c.defPx = new Vector2(px, py); c.size = size;
        c.pos = new Vector2(px / RefW, py / RefH);
        ctls.Add(c);
    }

    void BuildDefaults()
    {
        // left thumb: movement
        Add("dpad", "", Key.None, 270, 290, 330, true);
        // right thumb: actions
        Add("e", "E", Key.E, 2130, 290, 150);
        Add("enter", "Enter", Key.Enter, 2150, 480, 110);
        Add("shift", "Shift", Key.LeftShift, 1930, 215, 120);
        Add("zero", "0", Key.Digit0, 1950, 385, 115);
        Add("f", "F", Key.F, 1770, 170, 95);
        // top right: items and pause
        Add("n1", "1", Key.Digit1, 1960, 960, 84);
        Add("n2", "2", Key.Digit2, 2060, 960, 84);
        Add("n3", "3", Key.Digit3, 2160, 960, 84);
        Add("esc", "Esc", Key.Escape, 2300, 960, 76);
        // top left: info
        Add("alt", "Alt", Key.LeftAlt, 150, 940, 90);
        Add("r", "R", Key.R, 270, 940, 90);
        Add("tab", "Tab", Key.Tab, 390, 940, 90);
    }

    void ResetDefaults()
    {
        for (int i = 0; i < ctls.Count; i++)
        {
            ctls[i].pos = new Vector2(ctls[i].defPx.x / RefW, ctls[i].defPx.y / RefH);
            ctls[i].scale = 1f;
            ctls[i].alpha = DefaultAlpha;
        }
    }

    void LoadLayout()
    {
        try
        {
            string json = PlayerPrefs.GetString(PrefsKey, "");
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null || data.items == null) return;
            for (int i = 0; i < data.items.Count; i++)
            {
                var it = data.items[i];
                for (int j = 0; j < ctls.Count; j++)
                {
                    if (ctls[j].id != it.id) continue;
                    ctls[j].pos = new Vector2(Mathf.Clamp(it.x, 0.02f, 0.98f), Mathf.Clamp(it.y, 0.02f, 0.98f));
                    ctls[j].scale = Mathf.Clamp(it.scale, MinScale, MaxScale);
                    ctls[j].alpha = Mathf.Clamp(it.alpha, MinAlpha, MaxAlpha);
                }
            }
        }
        catch (Exception e) { Debug.LogError("Layout load failed: " + e.Message); }
    }

    void SaveLayout()
    {
        var data = new SaveData();
        for (int i = 0; i < ctls.Count; i++)
        {
            var it = new SaveItem();
            it.id = ctls[i].id; it.x = ctls[i].pos.x; it.y = ctls[i].pos.y;
            it.scale = ctls[i].scale; it.alpha = ctls[i].alpha;
            data.items.Add(it);
        }
        PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    void TakeSnapshot()
    {
        snapshot = new Vector4[ctls.Count];
        for (int i = 0; i < ctls.Count; i++)
            snapshot[i] = new Vector4(ctls[i].pos.x, ctls[i].pos.y, ctls[i].scale, ctls[i].alpha);
    }

    void RestoreSnapshot()
    {
        if (snapshot == null || snapshot.Length != ctls.Count) return;
        for (int i = 0; i < ctls.Count; i++)
        {
            ctls[i].pos = new Vector2(snapshot[i].x, snapshot[i].y);
            ctls[i].scale = snapshot[i].z;
            ctls[i].alpha = snapshot[i].w;
        }
    }

    // ------------------------------------------------------------------ procedural sprites
    static Sprite MakeSprite(int w, int h, Func<float, float, float> coverage, Vector4 border)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float a = Mathf.Clamp01(coverage(x + 0.5f, y + 0.5f));
                px[y * w + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
            }
        }
        tex.SetPixels32(px);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.Apply(false, true);
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }

    static float SDBox(float px, float py, float cx, float cy, float hx, float hy)
    {
        float qx = Mathf.Abs(px - cx) - hx, qy = Mathf.Abs(py - cy) - hy;
        float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
        return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f);
    }

    static float SMin(float a, float b, float k)
    {
        float h = Mathf.Clamp01(0.5f + 0.5f * (b - a) / k);
        return Mathf.Lerp(b, a, h) - k * h * (1f - h);
    }

    static float SegDist(float px, float py, float ax, float ay, float bx, float by)
    {
        float dx = bx - ax, dy = by - ay;
        float t = Mathf.Clamp01(((px - ax) * dx + (py - ay) * dy) / (dx * dx + dy * dy));
        float qx = ax + dx * t - px, qy = ay + dy * t - py;
        return Mathf.Sqrt(qx * qx + qy * qy);
    }

    static float EdgeIn(float px, float py, float ax, float ay, float bx, float by)
    {
        float ex = bx - ax, ey = by - ay;
        float len = Mathf.Sqrt(ex * ex + ey * ey);
        return (ex * (py - ay) - ey * (px - ax)) / len;
    }

    void BuildSprites()
    {
        sprCircle = MakeSprite(128, 128, (x, y) =>
        {
            float dx = x - 64f, dy = y - 64f;
            return 63f - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f;
        }, Vector4.zero);

        sprRing = MakeSprite(128, 128, (x, y) =>
        {
            float dx = x - 64f, dy = y - 64f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return Mathf.Clamp01(63f - d + 0.5f) * Mathf.Clamp01(d - 55f + 0.5f);
        }, Vector4.zero);

        sprRRect = MakeSprite(64, 64, (x, y) => 0.5f - (SDBox(x, y, 32f, 32f, 32f - 18f, 32f - 18f) - 18f), new Vector4(20, 20, 20, 20));

        sprArrow = MakeSprite(64, 64, (x, y) =>
        {
            float d1 = EdgeIn(x, y, 32f, 54f, 10f, 16f);
            float d2 = EdgeIn(x, y, 10f, 16f, 54f, 16f);
            float d3 = EdgeIn(x, y, 54f, 16f, 32f, 54f);
            return Mathf.Min(d1, Mathf.Min(d2, d3)) + 0.5f;
        }, Vector4.zero);

        sprGear = MakeSprite(128, 128, (x, y) =>
        {
            float dx = x - 64f, dy = y - 64f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float ang = Mathf.Atan2(dy, dx);
            float tooth = Mathf.Clamp01((Mathf.Cos(ang * 8f) + 0.25f) * 2.2f);
            float outer = 42f + 17f * tooth;
            return Mathf.Clamp01(outer - d + 0.5f) * Mathf.Clamp01(d - 19f + 0.5f);
        }, Vector4.zero);

        sprX = MakeSprite(128, 128, (x, y) =>
        {
            float d = Mathf.Min(SegDist(x, y, 26f, 26f, 102f, 102f), SegDist(x, y, 26f, 102f, 102f, 26f));
            return 8f - d + 0.5f;
        }, Vector4.zero);

        // water-drop notch: a flat top edge, a narrow neck and a round bulb
        sprNotch = MakeSprite(128, 96, (x, y) =>
        {
            float dx = x - 64f, dy = y - 40f;
            float dc = Mathf.Sqrt(dx * dx + dy * dy) - 34f;
            float dn = SDBox(x, y, 64f, 76f, 22f, 22f);
            float dt = SDBox(x, y, 64f, 92f, 46f, 6f);
            float d = SMin(SMin(dc, dn, 14f), dt, 16f);
            return 0.5f - d;
        }, Vector4.zero);
    }

    // ------------------------------------------------------------------ ui construction
    static RectTransform NewRT(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        return rt;
    }

    static Image NewImage(RectTransform rt, Sprite s, Color c, bool raycast)
    {
        var im = rt.gameObject.AddComponent<Image>();
        im.sprite = s; im.color = c; im.raycastTarget = raycast;
        return im;
    }

    static void Stretch(RectTransform rt, float inset)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset, inset); rt.offsetMax = new Vector2(-inset, -inset);
    }

    Text NewText(RectTransform rt, string s, int size, TextAnchor anchor, Color c)
    {
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font; t.fontSize = size; t.alignment = anchor; t.color = c;
        t.fontStyle = FontStyle.Bold; t.raycastTarget = false; t.text = s;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static void TopCenter(RectTransform rt, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0.5f, 1f); rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, y); rt.sizeDelta = new Vector2(w, h);
    }

    void BuildUI()
    {
        var root = new GameObject("PortTouchCanvas", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        root.AddComponent<GraphicRaycaster>();

        controlsRoot = NewRT("controls", root.transform);
        Stretch(controlsRoot, 0f);
        uiRoot = NewRT("ui", root.transform);
        Stretch(uiRoot, 0f);

        for (int i = 0; i < ctls.Count; i++) BuildCtl(ctls[i]);
        BuildNotch();
        BuildPopup();
        BuildSliders();
        BuildOptions();
    }

    void BuildCtl(Ctl c)
    {
        var rt = NewRT("ctl_" + c.id, controlsRoot);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(c.size, c.size);
        c.rt = rt;
        c.cg = rt.gameObject.AddComponent<CanvasGroup>();

        var glow = NewRT("glow", rt); Stretch(glow, -14f);
        c.glow = NewImage(glow, sprCircle, new Color(1f, 0.85f, 0.2f, 0.95f), false);
        c.glow.enabled = false;

        var bg = NewRT("bg", rt); Stretch(bg, 0f);
        c.bg = NewImage(bg, sprCircle, new Color(0f, 0f, 0f, 0.5f), true);

        var ring = NewRT("ring", rt); Stretch(ring, 0f);
        c.ring = NewImage(ring, sprRing, new Color(1f, 1f, 1f, 0.85f), false);

        if (c.dpad)
        {
            c.arrows = new Image[4];
            float[] ax = { 0f, 0f, -1f, 1f };
            float[] ay = { 1f, -1f, 0f, 0f };
            float[] rot = { 0f, 180f, 90f, -90f };
            for (int i = 0; i < 4; i++)
            {
                var a = NewRT("arrow" + i, rt);
                a.anchorMin = new Vector2(0.5f, 0.5f); a.anchorMax = new Vector2(0.5f, 0.5f); a.pivot = new Vector2(0.5f, 0.5f);
                a.sizeDelta = new Vector2(c.size * 0.26f, c.size * 0.26f);
                a.anchoredPosition = new Vector2(ax[i] * c.size * 0.33f, ay[i] * c.size * 0.33f);
                a.localEulerAngles = new Vector3(0f, 0f, rot[i]);
                c.arrows[i] = NewImage(a, sprArrow, new Color(1f, 1f, 1f, 0.6f), false);
            }
            var dot = NewRT("dot", rt);
            dot.anchorMin = new Vector2(0.5f, 0.5f); dot.anchorMax = new Vector2(0.5f, 0.5f); dot.pivot = new Vector2(0.5f, 0.5f);
            dot.sizeDelta = new Vector2(c.size * 0.14f, c.size * 0.14f);
            NewImage(dot, sprCircle, new Color(1f, 1f, 1f, 0.25f), false);
        }
        else
        {
            var lab = NewRT("label", rt);
            lab.anchorMin = new Vector2(0.12f, 0.12f); lab.anchorMax = new Vector2(0.88f, 0.88f);
            lab.offsetMin = Vector2.zero; lab.offsetMax = Vector2.zero;
            var t = NewText(lab, c.label, 40, TextAnchor.MiddleCenter, Color.white);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 10;
            t.resizeTextMaxSize = 60;
        }
    }

    void BuildNotch()
    {
        notchRT = NewRT("notch", uiRoot);
        TopCenter(notchRT, 0f, 100f, 75f);
        NewImage(notchRT, sprNotch, new Color(0.07f, 0.07f, 0.09f, 0.88f), true);
        var icon = NewRT("icon", notchRT);
        icon.anchorMin = new Vector2(0.5f, 0f); icon.anchorMax = new Vector2(0.5f, 0f); icon.pivot = new Vector2(0.5f, 0.5f);
        icon.anchoredPosition = new Vector2(0f, 31.7f);
        icon.sizeDelta = new Vector2(36f, 36f);   // same size as the gear in Free Fire's lobby
        notchIcon = NewImage(icon, sprGear, new Color(1f, 1f, 1f, 0.95f), false);
    }

    RectTransform Panel(string name, float w, float h)
    {
        var rt = NewRT(name, uiRoot);
        TopCenter(rt, -84f, w, h);
        NewImage(rt, sprRRect, new Color(0.07f, 0.07f, 0.09f, 0.9f), true).type = Image.Type.Sliced;
        rt.gameObject.SetActive(false);
        return rt;
    }

    void BuildPopup()
    {
        popupRT = Panel("popup", 360f, 60f);
        var t = NewRT("text", popupRT); Stretch(t, 0f);
        NewText(t, "Customize touch controls", 26, TextAnchor.MiddleCenter, Color.white);
    }

    void BuildSliders()
    {
        slidersRT = Panel("sliders", 440f, 140f);
        string[] names = { "Button size", "Opacity" };
        for (int i = 0; i < 2; i++)
        {
            var row = NewRT("row" + i, slidersRT);
            row.anchorMin = new Vector2(0f, 1f); row.anchorMax = new Vector2(1f, 1f); row.pivot = new Vector2(0.5f, 1f);
            row.anchoredPosition = new Vector2(0f, -10f - i * 62f); row.sizeDelta = new Vector2(0f, 58f);
            sliderRow[i] = row;

            var lab = NewRT("label", row);
            lab.anchorMin = new Vector2(0f, 0f); lab.anchorMax = new Vector2(0f, 1f); lab.pivot = new Vector2(0f, 0.5f);
            lab.anchoredPosition = new Vector2(16f, 0f); lab.sizeDelta = new Vector2(140f, 0f);
            NewText(lab, names[i], 22, TextAnchor.MiddleLeft, Color.white);

            var track = NewRT("track", row);
            track.anchorMin = new Vector2(0f, 0.5f); track.anchorMax = new Vector2(0f, 0.5f); track.pivot = new Vector2(0f, 0.5f);
            track.anchoredPosition = new Vector2(166f, 0f); track.sizeDelta = new Vector2(190f, 10f);
            NewImage(track, sprRRect, new Color(1f, 1f, 1f, 0.3f), false).type = Image.Type.Sliced;
            sliderTrack[i] = track;

            var fill = NewRT("fill", track);
            fill.anchorMin = new Vector2(0f, 0.5f); fill.anchorMax = new Vector2(0f, 0.5f); fill.pivot = new Vector2(0f, 0.5f);
            fill.anchoredPosition = Vector2.zero; fill.sizeDelta = new Vector2(100f, 10f);
            NewImage(fill, sprRRect, Pink, false).type = Image.Type.Sliced;
            sliderFill[i] = fill;

            var knob = NewRT("knob", track);
            knob.anchorMin = new Vector2(0f, 0.5f); knob.anchorMax = new Vector2(0f, 0.5f); knob.pivot = new Vector2(0.5f, 0.5f);
            knob.anchoredPosition = new Vector2(100f, 0f); knob.sizeDelta = new Vector2(34f, 34f);
            NewImage(knob, sprCircle, Color.white, false);
            sliderKnob[i] = knob;

            var val = NewRT("value", row);
            val.anchorMin = new Vector2(1f, 0f); val.anchorMax = new Vector2(1f, 1f); val.pivot = new Vector2(1f, 0.5f);
            val.anchoredPosition = new Vector2(-12f, 0f); val.sizeDelta = new Vector2(70f, 0f);
            sliderVal[i] = NewText(val, "100%", 20, TextAnchor.MiddleRight, new Color(1f, 1f, 1f, 0.8f));
        }
    }

    void BuildOptions()
    {
        optionsRT = Panel("options", 420f, 76f);
        string[] names = { "Default", "Save", "Cancel" };
        Color[] cols = { new Color(0.45f, 0.45f, 0.5f, 1f), Pink, new Color(0.75f, 0.25f, 0.25f, 1f) };
        for (int i = 0; i < 3; i++)
        {
            var b = NewRT("opt" + i, optionsRT);
            b.anchorMin = new Vector2(0.5f, 0.5f); b.anchorMax = new Vector2(0.5f, 0.5f); b.pivot = new Vector2(0.5f, 0.5f);
            b.anchoredPosition = new Vector2((i - 1) * 134f, 0f); b.sizeDelta = new Vector2(124f, 52f);
            NewImage(b, sprRRect, cols[i], true).type = Image.Type.Sliced;
            var t = NewRT("text", b); Stretch(t, 0f);
            NewText(t, names[i], 24, TextAnchor.MiddleCenter, Color.white);
            optBtn[i] = b;
        }
    }

    // ------------------------------------------------------------------ helpers
    bool Hit(RectTransform rt, Vector2 sp)
    {
        return rt != null && rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rt, sp, null);
    }

    float ScreenRadius(Ctl c)
    {
        return c.size * c.scale * canvas.scaleFactor * 0.5f;
    }

    Ctl HitCtl(Vector2 sp)
    {
        for (int i = ctls.Count - 1; i >= 0; i--)
        {
            var c = ctls[i];
            Vector2 centre = new Vector2(c.rt.position.x, c.rt.position.y);
            if ((sp - centre).magnitude <= ScreenRadius(c) * 1.05f) return c;
        }
        return null;
    }

    void UpdateDpad(Ctl c, Vector2 sp)
    {
        for (int i = 0; i < 4; i++) c.dir[i] = false;
        Vector2 centre = new Vector2(c.rt.position.x, c.rt.position.y);
        Vector2 v = sp - centre;
        if (v.magnitude < ScreenRadius(c) * 0.16f) return;
        Vector2 n = v.normalized;
        c.dir[0] = n.y > 0.38f;
        c.dir[1] = n.y < -0.38f;
        c.dir[2] = n.x < -0.38f;
        c.dir[3] = n.x > 0.38f;
    }

    // ------------------------------------------------------------------ modes
    void OpenPopup() { mode = Mode.Popup; }
    void ClosePopup() { if (mode == Mode.Popup) mode = Mode.Play; }

    void EnterEdit()
    {
        ReleaseAll();
        TakeSnapshot();
        mode = Mode.Edit;
        selected = null;
        optionsOpen = false;
    }

    void ExitEdit()
    {
        mode = Mode.Play;
        selected = null;
        optionsOpen = false;
        ReleaseAll();
    }

    void ReleaseAll()
    {
        caps.Clear();
        for (int i = 0; i < ctls.Count; i++)
        {
            ctls[i].down = false; ctls[i].touchId = -1;
            for (int k = 0; k < 4; k++) ctls[i].dir[k] = false;
        }
    }

    void Select(Ctl c)
    {
        selected = c;
        SyncSliders();
    }

    void SyncSliders()
    {
        if (selected == null) return;
        SetSliderVisual(0, Mathf.InverseLerp(MinScale, MaxScale, selected.scale), Mathf.RoundToInt(selected.scale * 100f) + "%");
        SetSliderVisual(1, Mathf.InverseLerp(MinAlpha, MaxAlpha, selected.alpha), Mathf.RoundToInt(selected.alpha * 100f) + "%");
    }

    void SetSliderVisual(int i, float v, string text)
    {
        float w = sliderTrack[i].sizeDelta.x;
        sliderFill[i].sizeDelta = new Vector2(v * w, 10f);
        sliderKnob[i].anchoredPosition = new Vector2(v * w, 0f);
        sliderVal[i].text = text;
    }

    void SetSliderFromTouch(int i, Vector2 sp)
    {
        if (selected == null) return;
        sliderTrack[i].GetWorldCorners(corners);
        float left = corners[0].x, right = corners[2].x;
        float v = Mathf.Clamp01((sp.x - left) / Mathf.Max(1f, right - left));
        if (i == 0) selected.scale = Mathf.Lerp(MinScale, MaxScale, v);
        else selected.alpha = Mathf.Lerp(MinAlpha, MaxAlpha, v);
        SyncSliders();
    }

    void ApplyOption(int i)
    {
        if (i == 0) { ResetDefaults(); PlayerPrefs.DeleteKey(PrefsKey); PlayerPrefs.Save(); }
        else if (i == 1) SaveLayout();
        else RestoreSnapshot();
        ExitEdit();
    }

    // ------------------------------------------------------------------ touch handling
    void OnBegan(int id, Vector2 p)
    {
        var cap = new Cap();
        caps[id] = cap;     // kind 0 = ignore unless changed below

        bool overNotch = Hit(notchRT, p);

        if (mode == Mode.Edit)
        {
            if (overNotch)
            {
                if (optionsOpen) optionsOpen = false;
                else { selected = null; optionsOpen = true; }
                return;
            }
            if (optionsOpen)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Hit(optBtn[i], p)) { ApplyOption(i); return; }
                }
                if (Hit(optionsRT, p)) return;
            }
            else if (selected != null && Hit(slidersRT, p))
            {
                for (int i = 0; i < 2; i++)
                {
                    if (Hit(sliderRow[i], p)) { cap.kind = 3; cap.slider = i; SetSliderFromTouch(i, p); return; }
                }
                return;
            }
            var hit = HitCtl(p);
            if (hit != null)
            {
                optionsOpen = false;
                Select(hit);
                cap.kind = 2; cap.ctl = hit;
                Vector2 centre = new Vector2(hit.rt.position.x, hit.rt.position.y);
                cap.grab = centre - p;
                return;
            }
            optionsOpen = false;
            selected = null;
            return;
        }

        if (overNotch)
        {
            if (mode == Mode.Popup) ClosePopup(); else OpenPopup();
            return;
        }
        if (mode == Mode.Popup)
        {
            if (Hit(popupRT, p)) { EnterEdit(); caps[id] = new Cap(); return; }
            ClosePopup();
        }
        var c = HitCtl(p);
        if (c != null)
        {
            cap.kind = 1; cap.ctl = c;
            c.down = true; c.touchId = id;
            if (c.dpad) UpdateDpad(c, p);
        }
    }

    void OnMoved(Cap cap, Vector2 p)
    {
        if (cap.kind == 1 && cap.ctl != null && cap.ctl.dpad) UpdateDpad(cap.ctl, p);
        else if (cap.kind == 2 && cap.ctl != null)
        {
            Vector2 centre = p + cap.grab;
            cap.ctl.pos = new Vector2(Mathf.Clamp(centre.x / Screen.width, 0.03f, 0.97f), Mathf.Clamp(centre.y / Screen.height, 0.04f, 0.96f));
        }
        else if (cap.kind == 3) SetSliderFromTouch(cap.slider, p);
    }

    void ReleaseCap(int id)
    {
        Cap cap;
        if (caps.TryGetValue(id, out cap))
        {
            if (cap.kind == 1 && cap.ctl != null)
            {
                cap.ctl.down = false; cap.ctl.touchId = -1;
                for (int k = 0; k < 4; k++) cap.ctl.dir[k] = false;
            }
            caps.Remove(id);
        }
    }

    void ProcessTouches()
    {
        seen.Clear();
        var ts = ETouch.activeTouches;
        for (int i = 0; i < ts.Count; i++)
        {
            var t = ts[i];
            int id = t.touchId;
            Vector2 p = t.screenPosition;
            bool ended = t.phase == UnityEngine.InputSystem.TouchPhase.Ended || t.phase == UnityEngine.InputSystem.TouchPhase.Canceled;
            if (ended) continue;
            seen.Add(id);
            Cap cap;
            if (!caps.TryGetValue(id, out cap))
            {
                if (t.began) OnBegan(id, p);
                else caps[id] = new Cap();      // touch that started before we were watching
            }
            else OnMoved(cap, p);
        }
        gone.Clear();
        foreach (var kv in caps) { if (!seen.Contains(kv.Key)) gone.Add(kv.Key); }
        for (int i = 0; i < gone.Count; i++) ReleaseCap(gone[i]);
    }

    // ------------------------------------------------------------------ keys
    void SendKeys()
    {
        want.Clear();
        if (visible && mode != Mode.Edit)
        {
            for (int i = 0; i < ctls.Count; i++)
            {
                var c = ctls[i];
                if (c.dpad)
                {
                    if (c.dir[0]) want.Add(Key.UpArrow);
                    if (c.dir[1]) want.Add(Key.DownArrow);
                    if (c.dir[2]) want.Add(Key.LeftArrow);
                    if (c.dir[3]) want.Add(Key.RightArrow);
                }
                else if (c.down) want.Add(c.key);
            }
        }
        if (want.SetEquals(sent)) return;
        sent.Clear();
        foreach (var k in want) sent.Add(k);
        if (kb == null) return;
        var arr = new Key[want.Count];
        want.CopyTo(arr);
        InputSystem.QueueStateEvent(kb, new KeyboardState(arr));
    }

    // ------------------------------------------------------------------ per frame
    void Update()
    {
        if (!visible)
        {
            SendKeys();
            return;
        }
        ProcessTouches();
        SendKeys();
        Refresh();
    }

    void Refresh()
    {
        float sf = Mathf.Max(0.01f, canvas.scaleFactor);
        float cw = Screen.width / sf, ch = Screen.height / sf;
        bool edit = mode == Mode.Edit;

        for (int i = 0; i < ctls.Count; i++)
        {
            var c = ctls[i];
            c.rt.localScale = new Vector3(c.scale, c.scale, 1f);
            c.rt.anchoredPosition = new Vector2(c.pos.x * cw, c.pos.y * ch);
            c.cg.alpha = c.alpha;
            c.glow.enabled = edit && selected == c;
            if (c.dpad)
            {
                c.bg.color = new Color(0f, 0f, 0f, 0.5f);
                for (int k = 0; k < 4; k++)
                    c.arrows[k].color = c.dir[k] ? new Color(Pink.r, Pink.g, Pink.b, 1f) : new Color(1f, 1f, 1f, 0.6f);
            }
            else c.bg.color = c.down ? new Color(Pink.r, Pink.g, Pink.b, 0.8f) : new Color(0f, 0f, 0f, 0.5f);
        }

        notchIcon.sprite = edit ? sprX : sprGear;
        popupRT.gameObject.SetActive(mode == Mode.Popup);
        optionsRT.gameObject.SetActive(edit && optionsOpen);
        slidersRT.gameObject.SetActive(edit && !optionsOpen && selected != null);
    }
}
