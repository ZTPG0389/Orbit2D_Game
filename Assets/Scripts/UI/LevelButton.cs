using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Wired by LevelSelectManager at spawn time
    [HideInInspector] public Image      outerGlow;
    [HideInInspector] public Image      innerPanel;
    [HideInInspector] public TMP_Text   numberText;
    [HideInInspector] public Image[]    starImages;
    [HideInInspector] public Sprite     sprStarFilled;
    [HideInInspector] public Sprite     sprStarEmpty;
    [HideInInspector] public GameObject lockOverlay;

    // Glow-ring colours
    static readonly Color GlowUnlocked = new Color(0.25f, 0.55f, 1.00f, 0.90f);
    static readonly Color GlowLocked   = new Color(0.12f, 0.12f, 0.28f, 0.40f);
    static readonly Color GlowCurrent  = new Color(0.00f, 0.90f, 1.00f, 1.00f);  // cyan pulse

    // Inner-face colours
    static readonly Color FaceUnlocked = new Color(0.08f, 0.18f, 0.52f, 1.00f);
    static readonly Color FaceLocked   = new Color(0.05f, 0.05f, 0.12f, 1.00f);
    static readonly Color FaceCurrent  = new Color(0.06f, 0.22f, 0.62f, 1.00f);

    private int       _level;
    private bool      _unlocked;
    private bool      _isCurrent;
    private Vector3   _baseScale;   // resting scale (already accounts for current-level up-scale)
    private Coroutine _tween;
    private Coroutine _pulse;

    // ── Public API ───────────────────────────────────────────
    public void Setup(int level, bool unlocked, int stars, bool isCurrent = false)
    {
        _level     = level;
        _unlocked  = unlocked;
        _isCurrent = isCurrent;

        // Capture GridLayout scale first, then apply current-level up-scale.
        // _baseScale becomes the permanent resting scale so all punch tweens
        // return to the right size.
        _baseScale = transform.localScale;
        if (isCurrent && unlocked)
        {
            _baseScale = _baseScale * 1.07f;
            transform.localScale = _baseScale;
        }

        // Colours
        if (outerGlow != null)
            outerGlow.color = isCurrent && unlocked ? GlowCurrent
                            : unlocked              ? GlowUnlocked
                            : GlowLocked;

        if (innerPanel != null)
            innerPanel.color = isCurrent && unlocked ? FaceCurrent
                             : unlocked              ? FaceUnlocked
                             : FaceLocked;

        // Number text
        if (numberText != null)
        {
            numberText.text  = level.ToString();
            numberText.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.28f);
        }

        // Stars
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] == null) continue;
                bool earned = unlocked && i < stars;
                starImages[i].sprite = earned ? sprStarFilled : sprStarEmpty;
                starImages[i].color  = earned
                    ? Color.white
                    : new Color(0.20f, 0.20f, 0.30f, 0.60f);
            }
        }

        // Lock overlay
        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        // Pulse glow for the current (next-to-play) level
        if (_pulse != null) StopCoroutine(_pulse);
        if (isCurrent && unlocked)
            _pulse = StartCoroutine(PulseGlow());
    }

    // Called by Button.onClick
    public void OnClick()
    {
        if (!_unlocked) return;
        if (_tween != null) StopCoroutine(_tween);
        if (_pulse  != null) StopCoroutine(_pulse);   // stop pulse during load
        StartCoroutine(PunchThenLoad());
    }

    public void OnPointerDown(PointerEventData _)
    {
        if (!_unlocked) return;
        PunchScale(0.90f);
    }

    public void OnPointerUp(PointerEventData _)
    {
        if (!_unlocked) return;
        PunchScale(1.00f);
    }

    // ── Animations ───────────────────────────────────────────
    private void PunchScale(float target)
    {
        if (_tween != null) StopCoroutine(_tween);
        _tween = StartCoroutine(TweenScale(_baseScale * target, 0.08f));
    }

    private IEnumerator PunchThenLoad()
    {
        yield return StartCoroutine(TweenScale(_baseScale * 0.88f, 0.07f));
        yield return StartCoroutine(TweenScale(_baseScale,         0.05f));
        Debug.Log($"[LevelButton] Loading Level {_level}");
        GameProgressManager.PlayLevel(_level);
    }

    private IEnumerator TweenScale(Vector3 to, float dur)
    {
        Vector3 from = transform.localScale;
        for (float t = 0f; t < dur; t += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(from, to, t / dur);
            yield return null;
        }
        transform.localScale = to;
    }

    // Breathing glow: pulses the outer ring alpha between 0.45 and 1.0
    private IEnumerator PulseGlow()
    {
        while (true)
        {
            if (outerGlow != null)
            {
                float f = (Mathf.Sin(Time.unscaledTime * 2.8f) + 1f) * 0.5f;
                Color c = outerGlow.color;
                c.a = Mathf.Lerp(0.45f, 1.00f, f);
                outerGlow.color = c;
            }
            yield return null;
        }
    }

}

