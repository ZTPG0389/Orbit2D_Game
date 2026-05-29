using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarUpdater : MonoBehaviour
{
    [SerializeField] Image           barFill;
    [SerializeField] TextMeshProUGUI progressText;

    Coroutine _animCoroutine;

    void Start()    => Refresh();
    void OnEnable() => Refresh();

    public void Refresh()
    {
        int   current    = FixedLevelManager.GetMaxUnlocked();
        int   total      = FixedLevelManager.GetTotalLevels(); // fixed: 50
        float targetFill = Mathf.Clamp01((float)current / total);

        if (barFill != null)
        {
            barFill.type       = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
        }

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateFill(targetFill, current, total));
    }

    IEnumerator AnimateFill(float targetFill, int current, int total)
    {
        const float Duration = 1.5f;
        float elapsed = 0f;

        if (barFill != null)      barFill.fillAmount = 0f;
        if (progressText != null) progressText.text  = $"0/{total}";

        while (elapsed < Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Duration);

            if (barFill != null)
                barFill.fillAmount = Mathf.Lerp(0f, targetFill, t);

            if (progressText != null)
            {
                int displayed = Mathf.RoundToInt(Mathf.Lerp(0f, current, t));
                progressText.text = $"{displayed}/{total}";
            }

            yield return null;
        }

        if (barFill != null)      barFill.fillAmount = targetFill;
        if (progressText != null) progressText.text  = $"{current}/{total}";
    }
}
