using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Prefab & Grid")]
    [SerializeField] GameObject      levelButtonPrefab;
    [SerializeField] Transform       gridParent;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI progressText;

    void Start()
    {
        PopulateGrid();
    }

    void PopulateGrid()
    {
        if (levelButtonPrefab == null)
        {
            Debug.LogError("[LevelSelectManager] levelButtonPrefab is not assigned.");
            return;
        }
        if (gridParent == null)
        {
            Debug.LogError("[LevelSelectManager] gridParent is not assigned.");
            return;
        }

        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        int total = FixedLevelManager.GetTotalLevels(); // always 50

        for (int i = 1; i <= total; i++)
        {
            var btn      = Instantiate(levelButtonPrefab, gridParent);
            bool unlocked = FixedLevelManager.IsUnlocked(i);

            var numberTransform = btn.transform.Find("Panel/Number");
            if (numberTransform != null)
            {
                var label = numberTransform.GetComponent<TextMeshProUGUI>();
                if (label != null) label.text = i.ToString();
            }

            var lockOverlay = btn.transform.Find("Panel/LockOverlay");
            if (lockOverlay != null)
                lockOverlay.gameObject.SetActive(!unlocked);

            var button = btn.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = unlocked;
                int levelIndex = i;
                button.onClick.AddListener(() => LevelLoader.LoadLevel(levelIndex));
            }
        }

        UpdateProgressText(total);
    }

    void UpdateProgressText(int total)
    {
        if (progressText == null) return;
        int unlocked = FixedLevelManager.GetMaxUnlocked();
        progressText.text = $"{unlocked}/{total}";
    }

    // ── Navigation ────────────────────────────────────────────────────────────
    public void BackToMainMenu() => SceneManager.LoadScene("MainMenu");

    // ── Static progression API (called by LevelCompleteUI / GameManager) ─────
    public static void UnlockNextLevel(int currentLevel)
    {
        FixedLevelManager.CompleteLevel(currentLevel);
    }

    public static void SaveLevelStars(int level, int stars)
    {
        string key  = $"OrbitDrop_Level_{level}_Stars";
        int    best = PlayerPrefs.GetInt(key, 0);
        if (stars > best)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }
}
