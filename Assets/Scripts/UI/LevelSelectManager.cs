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

        int total      = FixedLevelManager.GetTotalLevels();
        var sprFilled  = Resources.Load<Sprite>("Sprites/UI/star_filled");
        var sprEmpty   = Resources.Load<Sprite>("Sprites/UI/star_empty");

        for (int i = 1; i <= total; i++)
        {
            var  btn      = Instantiate(levelButtonPrefab, gridParent);
            bool unlocked = FixedLevelManager.IsUnlocked(i);

            var numberTransform = btn.transform.Find("Panel/Number");
            if (numberTransform != null)
            {
                var label = numberTransform.GetComponent<TextMeshProUGUI>();
                if (label != null) label.text = i.ToString();
            }

            var lockOverlay = btn.transform.Find("Panel/LockOverlay");
            if (lockOverlay != null) lockOverlay.gameObject.SetActive(!unlocked);

            int       starsEarned = PlayerPrefs.GetInt("Level_" + i + "_Stars", -1);
            Transform starsParent = btn.transform.Find("Panel/Stars");

            if (!unlocked)
            {
                if (starsParent != null) starsParent.gameObject.SetActive(false);
            }
            else if (starsEarned <= 0)
            {
                if (starsParent != null)
                {
                    starsParent.gameObject.SetActive(true);
                    ApplyStarSprite(starsParent, "Star1", sprEmpty);
                    ApplyStarSprite(starsParent, "Star2", sprEmpty);
                    ApplyStarSprite(starsParent, "Star3", sprEmpty);
                }
            }
            else
            {
                if (starsParent != null)
                {
                    starsParent.gameObject.SetActive(true);
                    ApplyStarSprite(starsParent, "Star1", starsEarned >= 1 ? sprFilled : sprEmpty);
                    ApplyStarSprite(starsParent, "Star2", starsEarned >= 2 ? sprFilled : sprEmpty);
                    ApplyStarSprite(starsParent, "Star3", starsEarned >= 3 ? sprFilled : sprEmpty);
                }
            }

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

    private static void ApplyStarSprite(Transform parent, string childName, Sprite sprite)
    {
        var t = parent.Find(childName);
        if (t == null) return;
        var img = t.GetComponent<Image>();
        if (img == null) return;
        img.sprite = sprite;
        img.color  = Color.white;
    }

    public static void SaveLevelStars(int level, int stars)
    {
        string key  = "Level_" + level + "_Stars";
        int    best = PlayerPrefs.GetInt(key, 0);
        if (stars > best)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }
}
