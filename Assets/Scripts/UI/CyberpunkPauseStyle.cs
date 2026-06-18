using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Attach to the "Card" child of PausePanel.
/// Applies the full cyberpunk sci-fi visual treatment at runtime.
/// PauseMenuUI.cs is untouched — all button callbacks remain intact.
[AddComponentMenu("UI/Cyberpunk Pause Style")]
public class CyberpunkPauseStyle : MonoBehaviour
{
    [Header("Optional — assign Orbitron Bold TMP Font Asset")]
    [SerializeField] TMP_FontAsset orbitronFont;

    // ── palette ───────────────────────────────────────────────────────────────
    //static readonly Color BgDark = C("071525");
    static readonly Color CyanFull = C("00e5ff");
    static readonly Color Gold = C("ffd700");
    static readonly Color BlueEdge = C("3366ff");
    static readonly Color BlueFace = C("4488ff");
    static readonly Color OrangeEdge = C("cc5500");
    static readonly Color OrangeFace = C("ff8800");
    static readonly Color HeartRed = C("ff2244");

    static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }

    // ── runtime refs ─────────────────────────────────────────────────────────
    CanvasGroup _parentGroup;
    float _prevAlpha;

    TextMeshProUGUI _titleTMP;
    Button _resumeBtn, _settingsBtn, _menuBtn;

    readonly GameObject[] _corners = new GameObject[4];
    readonly Transform[] _heartXf = new Transform[3];
    readonly Vector3[] _heartBase = new Vector3[3];

    Coroutine _entranceCo;
    Coroutine _cornerPulseCo;
    readonly Coroutine[] _heartCos = new Coroutine[3];
    CardPopupAnimation _cardPopup;

    bool _stylesApplied;

    // ── lifecycle ─────────────────────────────────────────────────────────────
    void Awake()
    {
        _parentGroup = GetComponentInParent<CanvasGroup>();
        _prevAlpha   = _parentGroup ? _parentGroup.alpha : 0f;
        _cardPopup   = GetComponent<CardPopupAnimation>();

        _titleTMP = transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        _resumeBtn = transform.Find("ResumeButton")?.GetComponent<Button>();
        _settingsBtn = transform.Find("SettingsButton")?.GetComponent<Button>();
        _menuBtn = transform.Find("MenuButton")?.GetComponent<Button>();

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find($"Heart{i + 1}");
            _heartXf[i] = go ? go.transform : null;
            _heartBase[i] = _heartXf[i] ? _heartXf[i].localScale : Vector3.one;
        }
    }

    void Start()
    {
        if (!_stylesApplied) { ApplyAllStyles(); _stylesApplied = true; }

        _cornerPulseCo = StartCoroutine(CornerPulse());

        for (int i = 0; i < 3; i++)
            if (_heartXf[i] != null)
                _heartCos[i] = StartCoroutine(HeartbeatLoop(i));
    }

    // Detects when the CanvasGroup becomes visible (PauseMenuUI.Show sets alpha=1)
    // Update runs every frame even while Time.timeScale == 0.
    void Update()
    {
        if (_parentGroup == null) return;
        float a = _parentGroup.alpha;
        if (_prevAlpha < 0.5f && a >= 0.5f)
        {
            // CardPopupAnimation owns localScale when present — skip PanelEntrance to avoid conflict.
            if (_cardPopup == null)
            {
                if (_entranceCo != null) StopCoroutine(_entranceCo);
                _entranceCo = StartCoroutine(PanelEntrance());
            }
        }
        _prevAlpha = a;
    }

    // ── style application ─────────────────────────────────────────────────────
    void ApplyAllStyles()
    {
        // ApplyPanelBg();
        ApplyTitle();
        ApplyButton(_resumeBtn, "RESUME",
            edgeColor: CyanFull,
            bgColor: new Color(0f, 0.898f, 1f, 0.06f));
        ApplyButton(_settingsBtn, "SETTINGS",
            edgeColor: BlueEdge,
            bgColor: new Color(0f, 0.314f, 1f, 0.06f));
        ApplyButton(_menuBtn, "MAIN MENU",
            edgeColor: OrangeEdge,
            bgColor: new Color(0.588f, 0.235f, 0f, 0.12f));
        CreateCircuitOverlay();
        CreateCornerBrackets();
        StyleHearts();
    }



    void ApplyTitle()
    {
        if (_titleTMP == null) return;

        // 🔒 padlock + PAUSED — fallback to text padlock if emoji font missing
        _titleTMP.text = "\U0001F512 PAUSED";
        _titleTMP.color = Color.white;
        _titleTMP.fontSize = 34f;
        _titleTMP.characterSpacing = 12f;
        _titleTMP.fontStyle = FontStyles.Bold;
        _titleTMP.alignment = TextAlignmentOptions.Center;
        _titleTMP.textWrappingMode = TextWrappingModes.NoWrap;
        if (orbitronFont != null) _titleTMP.font = orbitronFont;

        // TMP glow via material instance — "GLOW_ON" is the SDF shader keyword
        var mat = _titleTMP.fontMaterial;
        mat.EnableKeyword("GLOW_ON");
        mat.SetColor("_GlowColor", new Color(0f, 0.898f, 1f, 0.25f));
        mat.SetFloat("_GlowOuter", 0.30f);
        mat.SetFloat("_GlowPower", 0.50f);

        _titleTMP.outlineWidth = 0.15f;
        _titleTMP.outlineColor = new Color32(0, 229, 255, 60);
    }

    void ApplyButton(Button btn, string labelText, Color edgeColor, Color bgColor)
    {
        if (btn == null) return;

        // Panel background
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = bgColor;
            // Disable Unity's built-in tint transition — CyberpunkButtonEffect owns hover
            var cols = btn.colors;
            cols.normalColor = cols.highlightedColor =
            cols.pressedColor = cols.selectedColor = Color.white;
            btn.colors = cols;
        }

        // Border glow via Outline mesh effect
        var outline = btn.GetComponent<Outline>() ?? btn.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(edgeColor.r, edgeColor.g, edgeColor.b, 0.7f);
        outline.effectDistance = new Vector2(2f, 2f);
        outline.useGraphicAlpha = false;

        // Label
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = labelText;
            tmp.color = edgeColor;
            tmp.characterSpacing = 6f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 18f;
            tmp.alignment = TextAlignmentOptions.Center;
            if (orbitronFont != null) tmp.font = orbitronFont;
            tmp.outlineWidth = 0.15f;
            tmp.outlineColor = new Color(edgeColor.r, edgeColor.g, edgeColor.b, 0.4f);
        }

        // Attach per-button hover/glow component
        var effect = btn.GetComponent<CyberpunkButtonEffect>()
                  ?? btn.gameObject.AddComponent<CyberpunkButtonEffect>();
        effect.Init(img, outline, bgColor, edgeColor);
    }

    // ── decoration builders ───────────────────────────────────────────────────
    void CreateCircuitOverlay()
    {
        var go = new GameObject("CircuitOverlay",
                     typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var raw = go.GetComponent<RawImage>();
        raw.texture = GenCircuitTex();
        raw.uvRect = new Rect(0, 0, 22, 22); // ~28 px per tile on 600 px card
        raw.raycastTarget = false;
        raw.color = Color.white; // alpha is in the texture

        var le = go.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    void CreateCornerBrackets()
    {
        // Each corner gets its own correctly-oriented texture — no scale flipping.
        // flipX=false/true mirrors the horizontal arm; flipY=false/true mirrors the vertical arm.
        var textures = new Texture2D[]
        {
            GenCornerTex(false, false), // Corner0 — Top-Left
            GenCornerTex(true,  false), // Corner1 — Top-Right
            GenCornerTex(false, true),  // Corner2 — Bottom-Left
            GenCornerTex(true,  true),  // Corner3 — Bottom-Right
        };

        var anchors = new Vector2[] {
            new Vector2(0, 1),  // TL
            new Vector2(1, 1),  // TR
            new Vector2(0, 0),  // BL
            new Vector2(1, 0),  // BR
        };
        var pivots = new Vector2[] {
            new Vector2(0, 1),  // TL
            new Vector2(1, 1),  // TR
            new Vector2(0, 0),  // BL
            new Vector2(1, 0),  // BR
        };

        for (int i = 0; i < 4; i++)
        {
            var spr = Sprite.Create(textures[i],
                          new Rect(0, 0, textures[i].width, textures[i].height),
                          new Vector2(0.5f, 0.5f));

            var go = new GameObject($"Corner{i}",
                         typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin      = rt.anchorMax = anchors[i];
            rt.pivot          = pivots[i];
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta      = new Vector2(60, 60);
            rt.localScale     = Vector3.one;

            var img = go.GetComponent<Image>();
            img.sprite        = spr;
            img.type          = Image.Type.Simple;
            img.color         = CyanFull;
            img.raycastTarget = false;

            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            _corners[i] = go;
        }
    }

    void StyleHearts()
    {
        for (int i = 0; i < 3; i++)
        {
            if (_heartXf[i] == null) continue;
            var img = _heartXf[i].GetComponent<Image>();
            if (img) img.color = HeartRed;
        }
    }

    // ── coroutines ────────────────────────────────────────────────────────────
    IEnumerator PanelEntrance()
    {
        const float Duration = 0.35f;
        float elapsed = 0f;
        transform.localScale = Vector3.one * 0.85f;
        while (elapsed < Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / Duration), 2f); // ease-out quad
            transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.85f, Vector3.one, t);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    IEnumerator CornerPulse()
    {
        while (true)
        {
            // opacity oscillates 0.6 → 1.0 over 3 s
            float a = Mathf.Lerp(0.6f, 1.0f,
                          (Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / 3f)) + 1f) * 0.5f);
            for (int i = 0; i < 4; i++)
            {
                if (_corners[i] == null) continue;
                var img = _corners[i].GetComponent<Image>();
                if (img) img.color = new Color(CyanFull.r, CyanFull.g, CyanFull.b, a);
            }
            yield return null;
        }
    }

    IEnumerator HeartbeatLoop(int idx)
    {
        // Staggered start: 0 s / 0.3 s / 0.6 s
        yield return new WaitForSecondsRealtime(idx * 0.3f);
        var xf = _heartXf[idx];
        var baseScale = _heartBase[idx];

        while (true)
        {
            // Beat up: 1 → 1.2 over 0.15 s
            float e = 0f;
            while (e < 0.15f)
            {
                e += Time.unscaledDeltaTime;
                xf.localScale = baseScale * Mathf.LerpUnclamped(1f, 1.2f, e / 0.15f);
                yield return null;
            }
            // Beat down: 1.2 → 1 over 0.2 s
            e = 0f;
            while (e < 0.2f)
            {
                e += Time.unscaledDeltaTime;
                xf.localScale = baseScale * Mathf.LerpUnclamped(1.2f, 1f, e / 0.2f);
                yield return null;
            }
            xf.localScale = baseScale;
            yield return new WaitForSecondsRealtime(1.15f); // rest of 1.5 s cycle
        }
    }

    // ── texture generators ────────────────────────────────────────────────────
    static Texture2D GenCircuitTex()
    {
        const int S = 56;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat
        };
        var clear = new Color32(0, 0, 0, 0);
        var line = new Color32(0, 229, 255, 15);   // rgba(0,229,255, ~0.06)
        var pixels = new Color32[S * S];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        for (int x = 0; x < S; x++) pixels[0 * S + x] = line; // bottom row (y=0)
        for (int y = 0; y < S; y++) pixels[y * S + 0] = line; // left column (x=0)
        tex.SetPixels32(pixels);
        tex.Apply(false, false); // keep readable so it can be used as a sprite/texture
        return tex;
    }

    // flipX → mirror horizontal arm to the right side (TR / BR)
    // flipY → mirror vertical arm to the bottom (BL / BR)
    static Texture2D GenCornerTex(bool flipX, bool flipY)
    {
        const int S = 60;   // matches sizeDelta
        const int T = 3;    // line thickness (px)
        const int A = 22;   // arm length (px)

        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp
        };
        var pixels = new Color32[S * S];
        var clear  = new Color32(0, 0, 0, 0);
        var line   = new Color32(0, 229, 255, 255);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

        // In Unity Texture2D: y=0 = bottom, y=S-1 = top (matches UI display).
        // Horizontal arm sits at the top (flipY=false) or bottom (flipY=true).
        int hYStart = flipY ? 0     : S - T;
        int hYEnd   = flipY ? T     : S;
        int hXStart = flipX ? S - A : 0;
        int hXEnd   = flipX ? S     : A;

        // Vertical arm meets the horizontal arm — runs inward from the same corner.
        int vXStart = flipX ? S - T : 0;
        int vXEnd   = flipX ? S     : T;
        int vYStart = flipY ? 0     : S - A;
        int vYEnd   = flipY ? A     : S;

        for (int x = hXStart; x < hXEnd; x++)
            for (int y = hYStart; y < hYEnd; y++)
                pixels[y * S + x] = line;

        for (int x = vXStart; x < vXEnd; x++)
            for (int y = vYStart; y < vYEnd; y++)
                pixels[y * S + x] = line;

        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return tex;
    }
}
