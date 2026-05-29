using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelCompleteUI2D : MonoBehaviour
{
    public static LevelCompleteUI2D Instance;

    [Header("Panel")]
    [SerializeField] GameObject  panelRoot;

    [Header("Canvas Group")]
    [SerializeField] CanvasGroup group;

    [Header("Texts")]
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text subtitleText;
    [SerializeField] TMP_Text starsText;
    [SerializeField] TMP_Text bonusText;

    [Header("Buttons")]
    [SerializeField] Button nextLevelBtn;

    [Header("Stars Config")]
    [SerializeField] int targetScore = 1000;   // set per-level in Inspector if needed

    private int _currentLevel;

    void Awake()
    {
        Instance = this;

        // Hide via SetActive — never touch alpha in Awake so scene-saved alpha is preserved
        if (panelRoot != null) panelRoot.SetActive(false);

        // Keep alpha = 1 so the moment panelRoot is activated it is visible instantly
        if (group != null)
        {
            group.alpha          = 1f;
            group.interactable   = false;
            group.blocksRaycasts = false;
        }
    }

    void Start()
    {
        if (nextLevelBtn != null)
        {
            nextLevelBtn.onClick.AddListener(OnNextLevel);
        }
    }

    // Called by GameManager.OnLevelComplete()
    public void Show(int level, int bonus)
    {
        _currentLevel = level;

        // Activate panel first — alpha is already 1 from Awake
        if (panelRoot != null) panelRoot.SetActive(true);

        if (group != null)
        {
            group.alpha          = 1f;
            group.interactable   = true;
            group.blocksRaycasts = true;
        }

        // Texts
        int displayLevel = level > 0 ? level : PlayerPrefs.GetInt("SelectedLevel", 1);

        if (titleText != null)
            titleText.text = "You Win!";

        if (subtitleText != null)
            subtitleText.text = "Level " + displayLevel + " cleared!";

        if (bonusText != null)
            bonusText.text = "+" + bonus + " Bonus!";

        int stars = CalculateStars();
        if (starsText != null)
            starsText.text = BuildStarsText(stars);

        // Save progress
        LevelProgressData.UnlockNext(displayLevel);
        LevelProgressData.SetStars(displayLevel, stars);

        Debug.Log("[LevelCompleteUI2D] WinPanel shown — Level=" + displayLevel + " Stars=" + stars);

        StopAllCoroutines();
        StartCoroutine(AutoAdvance());
    }

    // Wired via AddListener and can also be set in Inspector onClick
    public void OnNextLevel()
    {
        StopAllCoroutines();
        int level = PlayerPrefs.GetInt("SelectedLevel", 1);
        PlayerPrefs.SetInt("SelectedLevel", level + 1);
        PlayerPrefs.Save();
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelected");
    }

    private IEnumerator AutoAdvance()
    {
        yield return new WaitForSecondsRealtime(2.5f);
        OnNextLevel();
    }

    public void Hide()
    {
        StopAllCoroutines();
        if (panelRoot != null) panelRoot.SetActive(false);
        // Do NOT reset alpha — keep it 1 so next Show() is instant
    }

    private int CalculateStars()
    {
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        if (score >= targetScore)              return 3;
        if (score >= targetScore * 0.7f)       return 2;
        if (score > 0)                         return 1;
        return 0;
    }

    private static string BuildStarsText(int stars)
    {
        const string on  = "<color=#FFD700>★</color>";
        const string off = "<color=#444444>★</color>";
        return (stars >= 1 ? on : off) +
               (stars >= 2 ? on : off) +
               (stars >= 3 ? on : off);
    }
}
