using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Attach to: the root of a full-screen "ScreenFlashCanvas" GameObject in the Game scene.
// That canvas needs: Canvas (Screen Space Overlay, Sort Order 99) + child Image stretched to full screen.
// Assign the Image to the flashImage field in the Inspector.
// Trigger with: ScreenFlash.Instance.Flash()
public class ScreenFlash : MonoBehaviour
{
    public static ScreenFlash Instance { get; private set; }

    [SerializeField] private Image flashImage;

    // Warm yellow-orange: gives a satisfying arcade impact feel
    [SerializeField] private Color flashColor = new Color(1f, 0.85f, 0.3f, 0.32f);
    [SerializeField] private float fadeInTime  = 0.04f;
    [SerializeField] private float fadeOutTime = 0.12f;

    private Coroutine _flashRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (flashImage != null)
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
    }

    // Quick screen flash. Call from CFXRExplosionManager on each hit.
    public void Flash(float overrideDuration = 0f)
    {
        if (flashImage == null) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        float dur = overrideDuration > 0f ? overrideDuration : fadeInTime + fadeOutTime;
        _flashRoutine = StartCoroutine(DoFlash(dur));
    }

    private IEnumerator DoFlash(float totalDuration)
    {
        float inT  = Mathf.Min(fadeInTime, totalDuration * 0.35f);
        float outT = totalDuration - inT;

        // Fade in
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / inT;
            SetAlpha(Mathf.Lerp(0f, flashColor.a, Mathf.Clamp01(t)));
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / outT;
            SetAlpha(Mathf.Lerp(flashColor.a, 0f, Mathf.Clamp01(t)));
            yield return null;
        }

        SetAlpha(0f);
        _flashRoutine = null;
    }

    private void SetAlpha(float a)
    {
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, a);
    }
}