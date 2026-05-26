using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────
    [Header("Config")]
    [SerializeField] private int totalLevels = 15;

    [Header("Scene References")]
    [SerializeField] private Transform gridParent;    // has GridLayoutGroup
    [SerializeField] private TMP_Text  progressText;  // "3/15"
    [SerializeField] private Image     progressFill;  // Image.Type.Filled, fillAmount 0–1

    // ── PlayerPrefs keys ─────────────────────────────────────
    // "UnlockedLevel" stores the highest level the player is allowed to play.
    // Default = 1 → only Level 1 unlocked at first run.
    const string KeyUnlocked = "UnlockedLevel";
    const string KeyStars    = "OrbitDrop_Level_{0}_Stars";

    // ── Colours (mirrored in LevelButton for run-time use) ───
    static readonly Color GlowUnlocked = new Color(0.25f, 0.55f, 1.00f, 0.90f);
    static readonly Color GlowLocked   = new Color(0.12f, 0.12f, 0.28f, 0.40f);
    static readonly Color GlowCurrent  = new Color(0.00f, 0.90f, 1.00f, 1.00f);
    static readonly Color FaceUnlocked = new Color(0.08f, 0.18f, 0.52f, 1.00f);
    static readonly Color FaceLocked   = new Color(0.05f, 0.05f, 0.12f, 1.00f);
    static readonly Color FaceCurrent  = new Color(0.06f, 0.22f, 0.62f, 1.00f);

    // ── Sprite rehydration (fills null refs on pre-built scene buttons) ──
    static Sprite _lockSprite, _starFilled, _starEmpty;

    static Sprite LoadSprite(string name)
    {
        var s = Resources.Load<Sprite>("Sprites/UI/" + name);
        if (s != null) return s;
        var t = Resources.Load<Texture2D>("Sprites/UI/" + name);
        return t != null
            ? Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100f)
            : null;
    }

    static Sprite GetStarFilled() { if (_starFilled == null) _starFilled = LoadSprite("star_filled"); return _starFilled; }
    static Sprite GetStarEmpty()  { if (_starEmpty  == null) _starEmpty  = LoadSprite("star_empty");  return _starEmpty;  }

    static Sprite GetLockSprite()
    {
        if (_lockSprite != null) return _lockSprite;

        // Best case: PNG was imported as Sprite type
        _lockSprite = Resources.Load<Sprite>("Sprites/UI/lock_icon");
        if (_lockSprite != null) return _lockSprite;

        // Fallback A: PNG present but imported as Texture2D (wrong import settings)
        var tex = Resources.Load<Texture2D>("Sprites/UI/lock_icon");
        if (tex != null)
        {
            _lockSprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            return _lockSprite;
        }

        // Fallback B: no file at all — draw a padlock procedurally so it is never blank
        _lockSprite = BuildLockSprite();
        return _lockSprite;
    }

    static Sprite BuildLockSprite()
    {
        const int S = 128;
        var tex  = new Texture2D(S, S, TextureFormat.RGBA32, false);
        var px   = new Color[S * S];
        var gold = new Color(1f, 0.71f, 0f, 1f);
        var dark = new Color(0.02f, 0.05f, 0.15f, 1f);
        var none = new Color(0, 0, 0, 0);

        for (int i = 0; i < px.Length; i++) px[i] = none;

        // Body
        for (int y = 10; y < 60; y++)
        for (int x = 35; x < 93; x++)
            px[y * S + x] = gold;

        // Shackle (hollow upper arc)
        for (int y = 60; y < S; y++)
        for (int x = 0; x < S; x++)
        {
            float d = Mathf.Sqrt((x - 64f) * (x - 64f) + (y - 60f) * (y - 60f));
            if (d >= 20f && d <= 28f) px[y * S + x] = gold;
        }

        // Keyhole
        for (int y = 0; y < S; y++)
        for (int x = 0; x < S; x++)
        {
            float d = Mathf.Sqrt((x - 64f) * (x - 64f) + (y - 35f) * (y - 35f));
            if (d <= 9f) px[y * S + x] = dark;
        }

        tex.SetPixels(px);
        tex.Apply(false);
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f);
    }

    // ── Lifecycle ────────────────────────────────────────────
    private void Start()
    {
        // Minimum guaranteed unlock is 1 so Level 1 is always playable.
        int unlocked = Mathf.Max(PlayerPrefs.GetInt(KeyUnlocked, 1), 1);
        BuildGrid(unlocked);
        RefreshProgress(unlocked);
    }

    // ── Grid updater — reads scene objects, never creates or destroys ──
    private void BuildGrid(int unlockedCount)
    {
        if (gridParent == null)
        {
            Debug.LogError("[LevelSelectManager] gridParent not assigned — drag Grid_Content here.");
            return;
        }

        int currentLevel = (unlockedCount <= totalLevels) ? unlockedCount : 0;

        for (int i = 1; i <= totalLevels; i++)
        {
            bool isUnlocked = i <= unlockedCount;
            bool isCurrent  = i == currentLevel;
            int  stars      = PlayerPrefs.GetInt(string.Format(KeyStars, i), 0);

            Transform t = gridParent.Find($"LvlBtn_{i:D2}");
            if (t == null)
            {
                Debug.LogWarning($"[LevelSelectManager] LvlBtn_{i:D2} not found in scene. " +
                                 "Run Tools > OrbitDrop > Populate Level Buttons (Preview) in Edit Mode first.");
                continue;
            }

            var lb = t.GetComponent<LevelButton>();
            if (lb == null)
            {
                Debug.LogWarning($"[LevelSelectManager] LvlBtn_{i:D2} has no LevelButton component.");
                continue;
            }

            // Rehydrate sprite refs that may be null if the scene was built before
            // sprites were generated (sprStarFilled/Empty are serialised but can be null).
            if (lb.sprStarFilled == null) lb.sprStarFilled = GetStarFilled();
            if (lb.sprStarEmpty  == null) lb.sprStarEmpty  = GetStarEmpty();

            // Lock-icon sprite lives on a grandchild Image — rehydrate if null.
            if (lb.lockOverlay != null)
            {
                var iconT = lb.lockOverlay.transform.Find("LockIcon");
                if (iconT != null)
                {
                    var iconImg = iconT.GetComponent<Image>();
                    if (iconImg != null && iconImg.sprite == null)
                        iconImg.sprite = GetLockSprite();
                }
            }

            // onClick listeners are not serialised — re-wire every Play session.
            var btn = t.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(lb.OnClick);
                btn.interactable = isUnlocked;
            }

            lb.Setup(i, isUnlocked, stars, isCurrent);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridParent as RectTransform);
    }

    // ── Progress bar ─────────────────────────────────────────
    private void RefreshProgress(int unlockedCount)
    {
        // Count levels where stars were awarded (= actually completed)
        int completed = 0;
        for (int i = 1; i <= totalLevels; i++)
            if (PlayerPrefs.GetInt(string.Format(KeyStars, i), 0) > 0)
                completed++;

        if (progressText != null)
            progressText.text = $"{completed}/{totalLevels}";

        if (progressFill != null)
            progressFill.fillAmount = totalLevels > 0 ? (float)completed / totalLevels : 0f;
    }

    // ── Navigation ────────────────────────────────────────────
    public void BackToMainMenu() => SceneManager.LoadScene("MainMenu");

    // ── Static progression API ────────────────────────────────
    // Call this from your LevelCompleteUI when the player finishes a level.
    //   currentLevel  = the level that was just beaten (1–15)
    public static void UnlockNextLevel(int currentLevel)
    {
        int unlocked = PlayerPrefs.GetInt(KeyUnlocked, 1);

        if (currentLevel >= unlocked)
        {
            int nextLevel = currentLevel + 1;
            PlayerPrefs.SetInt(KeyUnlocked, nextLevel);
            PlayerPrefs.Save();
            Debug.Log($"[LevelSelect] Unlocked Level {nextLevel}");
        }
        else
        {
            Debug.Log($"[LevelSelect] Level {currentLevel + 1} already unlocked — no change");
        }
    }

    // Call this to record the star rating for a completed level (keeps best score).
    public static void SaveLevelStars(int level, int stars)
    {
        string key  = string.Format(KeyStars, level);
        int    best = PlayerPrefs.GetInt(key, 0);
        if (stars > best)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    // ── Utility ──────────────────────────────────────────────
    static GameObject MakeChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }
}
