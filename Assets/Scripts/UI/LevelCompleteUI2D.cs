using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCompleteUI2D : MonoBehaviour
{
    public static LevelCompleteUI2D Instance;

    [SerializeField] CanvasGroup group;
    [SerializeField] TMP_Text    titleText;
    [SerializeField] TMP_Text    subtitleText;
    [SerializeField] TMP_Text    bonusText;
    [SerializeField] Button      nextLevelBtn; // hidden at runtime — kept so Inspector refs don't reset

    void Awake() => Instance = this;

    void Start()
    {
        Hide();
        if (nextLevelBtn != null) nextLevelBtn.gameObject.SetActive(false);
    }

    public void Show(int level, int bonus)
    {
        if (nextLevelBtn != null) nextLevelBtn.gameObject.SetActive(false);

        group.alpha          = 1f;
        group.interactable   = false;
        group.blocksRaycasts = false;

        titleText.text    = level >= 5 ? "You Win!" : "Level Complete!";
        subtitleText.text = "Level " + level + " cleared!";
        bonusText.text    = "+" + bonus + " Bonus!";

        StopAllCoroutines();
        StartCoroutine(AutoAdvance());
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        Hide();
        GameManager.Instance?.AdvanceLevel();
    }

    public void Hide()
    {
        StopAllCoroutines();
        group.alpha          = 0f;
        group.interactable   = false;
        group.blocksRaycasts = false;
    }
}
