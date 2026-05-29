using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardPopupAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float          duration       = 0.45f;
    public float          overshootScale = 1.12f;
    public bool           playOnEnable   = true;
    public AnimationCurve scaleCurve;

    [Header("Glow Flash")]
    public Image borderImage;
    public Color normalColor;
    public Color flashColor;
    public float flashDuration = 0.5f;

    CanvasGroup _canvasGroup;

    // Called when the component is first added in the Editor — sets default curve keys
    void Reset()
    {
        scaleCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.0f),
            new Keyframe(0.6f, 1.2f),
            new Keyframe(1.0f, 1.0f)
        );
    }

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (scaleCurve == null || scaleCurve.length == 0)
            Reset();

        if (borderImage != null)
            normalColor = borderImage.color;

        Debug.Log("CardPopupAnimation ready on " + gameObject.name);
    }

    void OnEnable()
    {
        if (!playOnEnable) return;
        StopAllCoroutines();
        StartCoroutine(AnimatePopup());
        StartCoroutine(FlashBorder());
    }

    public void PlayAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(AnimatePopup());
        StartCoroutine(FlashBorder());
    }

    IEnumerator AnimatePopup()
    {
        float   phase1    = duration * 0.7f;
        float   phase2    = duration * 0.3f;
        Vector3 startScale  = Vector3.zero;
        Vector3 overshoot   = Vector3.one * overshootScale;

        _canvasGroup.alpha       = 0f;
        transform.localScale = startScale;

        // Phase 1 — scale up to overshoot, fade in
        float elapsed = 0f;
        while (elapsed < phase1)
        {
            elapsed += Time.unscaledDeltaTime;
            float t      = Mathf.Clamp01(elapsed / phase1);
            float curveT = scaleCurve.Evaluate(t);

            transform.localScale = Vector3.LerpUnclamped(startScale, overshoot, curveT);
            _canvasGroup.alpha   = Mathf.Lerp(0f, 1f, t * 2f);
            yield return null;
        }

        _canvasGroup.alpha = 1f;

        // Phase 2 — settle back to normal scale
        elapsed = 0f;
        while (elapsed < phase2)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / phase2);
            transform.localScale = Vector3.Lerp(overshoot, Vector3.one, t);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    IEnumerator FlashBorder()
    {
        if (borderImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t          = elapsed / flashDuration;
            float brightness = Mathf.Sin(t * Mathf.PI); // 0 → peak → 0
            borderImage.color = Color.Lerp(normalColor, flashColor, brightness);
            yield return null;
        }

        borderImage.color = normalColor;
    }
}
