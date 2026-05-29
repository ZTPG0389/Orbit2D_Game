using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GamePanelLiftEffect : MonoBehaviour
{
    [Header("Animation")]
    public float popupDuration = 0.45f;
    public float floatAmount   = 4f;
    public float floatSpeed    = 1.2f;

    [Header("References")]
    public Image backgroundOverlay;
    public Image cardBorderImage;

    CanvasGroup   _cg;
    RectTransform _rt;
    RectTransform _shadowRt;
    Vector2       _basePos;

    // ── lifecycle ──────────────────────────────────────────────────────────────
    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();
        CreateShadow();
    }

    void OnEnable()
    {
        _basePos = _rt.anchoredPosition;

        StopAllCoroutines();
        StartCoroutine(PopupAnimation());

        if (backgroundOverlay != null)
            StartCoroutine(FadeOverlay(0f, 0.6f, 0.3f));

        if (cardBorderImage != null)
            StartCoroutine(BorderPulse());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        _rt.anchoredPosition = _basePos;
        _rt.localScale       = Vector3.one;
        _cg.alpha            = 1f;

        if (backgroundOverlay != null)
            backgroundOverlay.color = new Color(0f, 0f, 0f, 0f);
    }

    // ── Effect 1: drop shadow ─────────────────────────────────────────────────
    void CreateShadow()
    {
        // Avoid duplicates across domain reloads / multiple Awake calls
        var existing = transform.parent != null ? transform.parent.Find("Shadow") : null;
        if (existing != null)
        {
            _shadowRt = existing.GetComponent<RectTransform>();
            return;
        }

        var go = new GameObject("Shadow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform.parent, false);
        go.transform.SetSiblingIndex(0); // behind every sibling

        _shadowRt            = go.GetComponent<RectTransform>();
        _shadowRt.anchorMin  = _rt.anchorMin;
        _shadowRt.anchorMax  = _rt.anchorMax;
        _shadowRt.pivot      = _rt.pivot;
        _shadowRt.sizeDelta  = _rt.sizeDelta + new Vector2(20f, 20f); // +10 per side
        _shadowRt.anchoredPosition = _rt.anchoredPosition + new Vector2(8f, -8f);

        var img = go.GetComponent<Image>();
        img.color         = new Color(0f, 0f, 0f, 0.5f);
        img.raycastTarget = false;

        var le = go.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    // ── Effect 2: popup animation ─────────────────────────────────────────────
    IEnumerator PopupAnimation()
    {
        _cg.alpha            = 0f;
        transform.localScale = Vector3.one * 0.75f;

        // Phase 1 — 0 → 0.25s: scale 0.75 → 1.08, alpha 0 → 1
        yield return AnimatePhase(0.75f, 1.08f, 0f, 1f, 0.25f);

        // Phase 2 — 0.25 → 0.38s: scale 1.08 → 0.97
        yield return AnimatePhase(1.08f, 0.97f, 1f, 1f, 0.13f);

        // Phase 3 — 0.38 → 0.45s: scale 0.97 → 1.0
        yield return AnimatePhase(0.97f, 1.0f, 1f, 1f, 0.07f);

        transform.localScale = Vector3.one;
        _cg.alpha            = 1f;

        // Hand off to idle float
        StartCoroutine(IdleFloat());
    }

    IEnumerator AnimatePhase(float s0, float s1, float a0, float a1, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / dur));
            transform.localScale = Vector3.one * Mathf.Lerp(s0, s1, t);
            _cg.alpha            = Mathf.Lerp(a0, a1, t);
            yield return null;
        }
    }

    // ── Effect 3: idle float ──────────────────────────────────────────────────
    IEnumerator IdleFloat()
    {
        while (true)
        {
            float y = Mathf.Sin(Time.unscaledTime * floatSpeed) * floatAmount;
            _rt.anchoredPosition = _basePos + new Vector2(0f, y);
            yield return null;
        }
    }

    // ── Effect 4: background overlay fade ────────────────────────────────────
    IEnumerator FadeOverlay(float from, float to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            backgroundOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, t));
            yield return null;
        }
        backgroundOverlay.color = new Color(0f, 0f, 0f, to);
    }

    // ── Effect 5: border glow pulse ───────────────────────────────────────────
    IEnumerator BorderPulse()
    {
        Color c = cardBorderImage.color;
        while (true)
        {
            // Sin oscillates -1→1; map to 0→1 then lerp alpha 0.7→1.0
            float t     = (Mathf.Sin(Time.unscaledTime * Mathf.PI) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(0.7f, 1.0f, t);
            cardBorderImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
    }
}
