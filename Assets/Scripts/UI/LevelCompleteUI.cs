using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using DG.Tweening; // re-enable after DOTween import (Step 8)

// Step 37 — DOTween Sequence fade+scale in; auto-hide after 1.7s delay
public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private TMP_Text    levelText;
    [SerializeField] private TMP_Text    bonusText;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private GameObject  panelRoot;

    private const float FadeInDuration  = 0.30f;
    private const float HoldDuration    = 1.70f;
    private const float FadeOutDuration = 0.22f;

    private void Awake()
    {
        SetGroupVisible(false);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.LevelComplete)
            Show();
        else
            Hide();
    }

    private void Show()
    {
        if (panelRoot != null) panelRoot.SetActive(true);

        int level = GameManager.Instance?.Level ?? 0;
        if (levelText != null) levelText.text = $"LEVEL {level}  COMPLETE";
        if (bonusText != null) bonusText.text  = "+500";
        StopAllCoroutines();
        StartCoroutine(Sequence());
    }

    private void Hide()
    {
        StopAllCoroutines();
        SetGroupVisible(false);
    }

    private IEnumerator Sequence()
    {
        if (group == null) yield break;

        // Fade + scale in (ease-out cubic)
        transform.localScale = Vector3.one * 0.65f;
        group.alpha          = 0f;
        group.interactable   = true;
        group.blocksRaycasts = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / FadeInDuration;
            float e          = EaseOutCubic(Mathf.Clamp01(t));
            group.alpha      = e;
            transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.65f, Vector3.one, e);
            yield return null;
        }
        group.alpha          = 1f;
        transform.localScale = Vector3.one;

        // Hold
        yield return new WaitForSecondsRealtime(HoldDuration);

        // Fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / FadeOutDuration;
            group.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t));
            yield return null;
        }

        SetGroupVisible(false);
        GameManager.Instance?.AdvanceLevel();
    }

    private void SetGroupVisible(bool visible)
    {
        if (group == null) return;
        group.alpha          = visible ? 1f : 0f;
        group.interactable   = visible;
        group.blocksRaycasts = visible;
    }

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
}
