using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Added dynamically by CyberpunkPauseStyle to each pause-menu button.
/// Drives glow-pulse animation and hover / press scale effects.
[AddComponentMenu("UI/Cyberpunk Button Effect")]
public class CyberpunkButtonEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler,  IPointerUpHandler
{
    Image   _bgImg;
    Outline _outline;
    Color   _bgBase;
    Color   _edgeColor;

    Coroutine _pulseCo;
    Coroutine _scaleCo;

    bool _ready; // true once Init() has supplied all references

    // Called by CyberpunkPauseStyle immediately after AddComponent
    public void Init(Image bg, Outline outline, Color bgBase, Color edge)
    {
        _bgImg     = bg;
        _outline   = outline;
        _bgBase    = bgBase;
        _edgeColor = edge;
        _ready     = true;

        if (_pulseCo != null) StopCoroutine(_pulseCo);
        _pulseCo = StartCoroutine(GlowPulse());
    }

    void OnEnable()
    {
        // Re-start after the GameObject is re-enabled (e.g. scene reload)
        if (!_ready) return;
        if (_pulseCo != null) StopCoroutine(_pulseCo);
        _pulseCo = StartCoroutine(GlowPulse());
    }

    void OnDisable()
    {
        if (_pulseCo != null) { StopCoroutine(_pulseCo); _pulseCo = null; }
        if (_scaleCo != null) { StopCoroutine(_scaleCo); _scaleCo = null; }
    }

    // ── pointer events ────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData _) => AnimateTo(1.02f, 0.12f);
    public void OnPointerExit(PointerEventData _)  => AnimateTo(1.00f, 0.12f);
    public void OnPointerDown(PointerEventData _)  => AnimateTo(0.97f, 0.08f);
    public void OnPointerUp(PointerEventData _)    => AnimateTo(1.02f, 0.08f);

    void AnimateTo(float target, float dur)
    {
        if (_scaleCo != null) StopCoroutine(_scaleCo);
        _scaleCo = StartCoroutine(ScaleTo(target, dur));
    }

    // ── coroutines ────────────────────────────────────────────────────────────

    // Oscillates the Outline border alpha: 0.35 ↔ 0.9 over 2 s
    IEnumerator GlowPulse()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * Mathf.PI) + 1f) * 0.5f; // 0..1, 2 s period
            if (_outline != null)
            {
                float a = Mathf.Lerp(0.35f, 0.9f, t);
                _outline.effectColor = new Color(_edgeColor.r, _edgeColor.g, _edgeColor.b, a);
            }
            yield return null;
        }
    }

    IEnumerator ScaleTo(float target, float duration)
    {
        Vector3 from = transform.localScale;
        Vector3 to   = new Vector3(target, target, 1f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        transform.localScale = to;
    }
}
