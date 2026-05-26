using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectedBuilder : MonoBehaviour
{
    // Cached sprites loaded once from Resources/Sprites/UI/
    Sprite _sprBtn;
    Sprite _sprLock;
    Sprite _sprStarFilled;
    Sprite _sprStarEmpty;
    Sprite _sprBg;

    void Start()
    {
        // Yield to LevelSelectManager when both scripts are present in the scene
        if (FindObjectOfType<LevelSelectManager>() != null) return;

        _sprBtn        = Resources.Load<Sprite>("Sprites/UI/level_button");
        _sprLock       = Resources.Load<Sprite>("Sprites/UI/lock_icon");
        _sprStarFilled = Resources.Load<Sprite>("Sprites/UI/star_filled");
        _sprStarEmpty  = Resources.Load<Sprite>("Sprites/UI/star_empty");
        _sprBg         = Resources.Load<Sprite>("Sprites/UI/space_bg");

        BuildGrid();
        UpdateHeader();
        UpdateBackground();
    }

    // ── Grid ─────────────────────────────────────────────────────
    private void BuildGrid()
    {
        GameObject content = GameObject.Find("Grid_Content");
        if (content == null) { Debug.LogError("[LevelSelectedBuilder] Grid_Content not found!"); return; }

        foreach (Transform child in content.transform)
            Destroy(child.gameObject);

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>()
            ?? content.AddComponent<GridLayoutGroup>();
        grid.cellSize        = new Vector2(200, 200);
        grid.spacing         = new Vector2(20, 20);
        grid.padding         = new RectOffset(30, 30, 30, 30);
        grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment  = TextAnchor.UpperCenter;

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>()
            ?? content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        int unlockedLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);

        for (int i = 1; i <= 15; i++)
        {
            bool unlocked = i <= unlockedLevel;
            int  stars    = PlayerPrefs.GetInt("Level_" + i + "_Stars", 0);
            SpawnButton(content.transform, i, unlocked, stars);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        Debug.Log("[LevelSelectedBuilder] Built 15 buttons. Unlocked up to: " + unlockedLevel);
    }

    // ── Button factory ────────────────────────────────────────────
    private void SpawnButton(Transform parent, int i, bool unlocked, int stars)
    {
        // Root
        var btnGO = new GameObject("LvlBtn_" + i);
        btnGO.transform.SetParent(parent, false);

        var bg = btnGO.AddComponent<Image>();
        if (_sprBtn != null)
        {
            bg.sprite = _sprBtn;
            bg.type   = Image.Type.Sliced;
            bg.color  = unlocked
                ? new Color(0.10f, 0.35f, 0.85f, 1.00f)
                : new Color(0.05f, 0.10f, 0.25f, 0.85f);
        }
        else
        {
            bg.color = unlocked
                ? new Color(0.10f, 0.35f, 0.85f, 1.00f)
                : new Color(0.05f, 0.10f, 0.25f, 0.85f);
        }

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(0.80f, 0.90f, 1.00f, 1f);
        cb.pressedColor     = new Color(0.60f, 0.70f, 0.90f, 1f);
        cb.colorMultiplier  = 1f;
        btn.colors = cb;

        if (unlocked)
            BuildUnlockedContent(btnGO.transform, i, stars);
        else
            BuildLockedContent(btnGO.transform, i);

        // Click handler
        int   levelNum   = i;
        bool  isUnlocked = unlocked;
        btn.onClick.AddListener(() =>
        {
            if (!isUnlocked) return;
            PlayerPrefs.SetInt("SelectedLevel", levelNum);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Game");
        });
    }

    // ── Unlocked: large number + cyan stars ───────────────────────
    private void BuildUnlockedContent(Transform parent, int level, int stars)
    {
        // Number
        var numGO  = MakeRect(parent, "Number");
        var numRT  = numGO.GetComponent<RectTransform>();
        numRT.anchorMin = new Vector2(0f,    0.35f);
        numRT.anchorMax = new Vector2(1f,    1.00f);
        numRT.offsetMin = numRT.offsetMax = Vector2.zero;
        var numTmp = numGO.AddComponent<TextMeshProUGUI>();
        numTmp.text      = level.ToString();
        numTmp.fontSize  = 52;
        numTmp.fontStyle = FontStyles.Bold;
        numTmp.color     = Color.white;
        numTmp.alignment = TextAlignmentOptions.Center;

        // Stars row
        var starsGO = MakeRect(parent, "Stars");
        var starsRT = starsGO.GetComponent<RectTransform>();
        starsRT.anchorMin = new Vector2(0.05f, 0.00f);
        starsRT.anchorMax = new Vector2(0.95f, 0.38f);
        starsRT.offsetMin = starsRT.offsetMax = Vector2.zero;
        var hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment        = TextAnchor.MiddleCenter;
        hlg.spacing               = 6f;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight= false;

        for (int s = 1; s <= 3; s++)
        {
            var starGO  = MakeRect(starsGO.transform, "Star" + s);
            var starRT  = starGO.GetComponent<RectTransform>();
            starRT.sizeDelta = new Vector2(32, 32);
            var starImg = starGO.AddComponent<Image>();
            bool earned = s <= stars;
            if (earned && _sprStarFilled != null)
                starImg.sprite = _sprStarFilled;
            else if (!earned && _sprStarEmpty != null)
                starImg.sprite = _sprStarEmpty;
            starImg.color = earned
                ? new Color(0.00f, 0.90f, 1.00f, 1.00f)   // cyan — earned
                : new Color(0.20f, 0.20f, 0.30f, 0.60f);  // dark — empty
        }
    }

    // ── Locked: gold lock icon + small number ────────────────────
    private void BuildLockedContent(Transform parent, int level)
    {
        // Lock icon
        var lockGO  = MakeRect(parent, "LockIcon");
        var lockRT  = lockGO.GetComponent<RectTransform>();
        lockRT.anchorMin = new Vector2(0.20f, 0.20f);
        lockRT.anchorMax = new Vector2(0.80f, 0.85f);
        lockRT.offsetMin = lockRT.offsetMax = Vector2.zero;
        var lockImg = lockGO.AddComponent<Image>();
        lockImg.color = new Color(1f, 0.75f, 0.10f, 0.90f);
        if (_sprLock != null) lockImg.sprite = _sprLock;

        // Small level number at bottom
        var snGO  = MakeRect(parent, "SmallNum");
        var snRT  = snGO.GetComponent<RectTransform>();
        snRT.anchorMin = new Vector2(0f, 0.00f);
        snRT.anchorMax = new Vector2(1f, 0.25f);
        snRT.offsetMin = snRT.offsetMax = Vector2.zero;
        var snTmp = snGO.AddComponent<TextMeshProUGUI>();
        snTmp.text      = level.ToString();
        snTmp.fontSize  = 18;
        snTmp.color     = new Color(0.50f, 0.60f, 0.70f, 0.80f);
        snTmp.alignment = TextAlignmentOptions.Center;
    }

    // ── Header ───────────────────────────────────────────────────
    private void UpdateHeader()
    {
        int unlockedLevel = PlayerPrefs.GetInt("HighestUnlockedLevel", 1);
        int completed     = Mathf.Max(0, unlockedLevel - 1);

        var progressText = GameObject.Find("Progress_Text");
        if (progressText != null)
        {
            var pt = progressText.GetComponent<TextMeshProUGUI>();
            if (pt != null) pt.text = completed + "/15 COMPLETED";
        }

        var titleText = GameObject.Find("Title_LEVELS");
        if (titleText != null)
        {
            var tt = titleText.GetComponent<TextMeshProUGUI>();
            if (tt != null) tt.text = "LEVELS";
        }

        var barFill = GameObject.Find("Bar_Fill");
        if (barFill != null)
        {
            var img = barFill.GetComponent<Image>();
            if (img != null) img.fillAmount = completed / 15f;
        }
    }

    // ── Background ───────────────────────────────────────────────
    private void UpdateBackground()
    {
        var bg = GameObject.Find("Background");
        if (bg == null) return;
        var img = bg.GetComponent<Image>();
        if (img == null) return;
        if (_sprBg != null)
        {
            img.sprite = _sprBg;
            img.type   = Image.Type.Simple;
            img.preserveAspect = false;
        }
        img.color = new Color(0.03f, 0.06f, 0.15f, 1f);
    }

    // ── Back button (wire in Inspector) ──────────────────────────
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // ── Utility ─────────────────────────────────────────────────
    private static GameObject MakeRect(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }
}
