using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// Drives Playbutton click-feedback scale animation.
/// Works alongside the Animator (idle pulse) without conflict:
///   - PointerDown  → disables Animator, squishes to 0.92 in 0.1s
///   - PointerUp    → returns to 1.0 in 0.1s, re-enables Animator at normalizedTime=0
///     so the idle pulse resumes cleanly from scale 1.0 with no visible jump.
[RequireComponent(typeof(Animator))]
public class PlaybuttonAnimator : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float pressedScale   = 0.92f;
    [SerializeField] float clickDuration  = 0.10f;   // seconds for press AND release

    Animator   _anim;
    Coroutine  _cr;

    void Awake() => _anim = GetComponent<Animator>();

    // ── Pointer events ──────────────────────────────────────────────────────

    public void OnPointerDown(PointerEventData _)
    {
        // Freeze idle pulse while pressed so Animator doesn't fight the script.
        _anim.enabled = false;
        ScaleTo(pressedScale);
    }

    public void OnPointerUp(PointerEventData _)
    {
        // Return to 1.0 then hand control back to the Animator.
        ScaleTo(1f, onDone: ResumeAnimator);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    void ScaleTo(float target, System.Action onDone = null)
    {
        if (_cr != null) StopCoroutine(_cr);
        _cr = StartCoroutine(ScaleRoutine(target, onDone));
    }

    IEnumerator ScaleRoutine(float target, System.Action onDone)
    {
        Vector3 start = transform.localScale;
        Vector3 end   = new Vector3(target, target, 1f);
        float   t     = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / clickDuration;
            // SmoothStep gives an ease-in/out feel for a premium press response.
            float s = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            transform.localScale = Vector3.LerpUnclamped(start, end, s);
            yield return null;
        }

        transform.localScale = end;
        _cr = null;
        onDone?.Invoke();
    }

    void ResumeAnimator()
    {
        // Re-entering at normalizedTime = 0 guarantees the first animated
        // value the Animator writes is scale 1.0, matching where we just landed.
        _anim.enabled = true;
        _anim.Play("Idle", 0, 0f);
    }
}
