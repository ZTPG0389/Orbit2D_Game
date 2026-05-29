using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectedBuilder : MonoBehaviour
{
    Sprite _sprBtn;
    Sprite _sprLock;
    Sprite _sprStarFilled;
    Sprite _sprStarEmpty;
    Sprite _sprBg;

    void Start()
    {
        if (FindObjectOfType<LevelSelectManager>() != null) return;

        _sprBtn        = Resources.Load<Sprite>("Sprites/UI/level_button");
        _sprLock       = Resources.Load<Sprite>("Sprites/UI/lock_icon");
        _sprStarFilled = Resources.Load<Sprite>("Sprites/UI/star_filled");
        _sprStarEmpty  = Resources.Load<Sprite>("Sprites/UI/star_empty");
        _sprBg         = Resources.Load<Sprite>("Sprites/UI/space_bg");

        FixScrollView();
        BuildGrid();
        UpdateHeader();
        UpdateBackground();
    }

    // ── 1. Fix ScrollRect + Viewport + content anchor ────────────
    private void FixScrollView()
    {
        // ScrollRect
        GameObject scrollGO = GameObject.Find("LevelScroll");
        if (scrollGO != null)
        {
            ScrollRect sr = scrollGO.GetComponent<ScrollRect>();
            if (sr != null)
            {
                sr.vertical          = true;
                sr.horizontal        = false;
                sr.movementType      = ScrollRect.MovementType.Elastic;
                sr.scrollSensitivity = 30f;
                sr.inertia           = true;
                sr.decelerationRate  = 0.135f;
            }

            // Viewport — stretch-fill the scroll rect, add Mask
            Transform viewportT = scrollGO.transform.Find("Viewport");
            if (viewportT != null)
            {
                RectTransform vrt = viewportT.GetComponent<RectTransform>();
                vrt.anchorMin = Vector2.zero;
                vrt.anchorMax = Vector2.one;
                vrt.offsetMin = Vector2.zero;
                vrt.offsetMax = Vector2.zero;

                Mask mask = viewportT.GetComponent<Mask>() ?? viewportT.gameObject.AddComponent<Mask>();
                mask.showMaskGraphic = false;

                // Viewport needs an Image for the Mask to work
                if (viewportT.GetComponent<Image>() == null)
                {
                    var img = viewportT.gameObject.AddComponent<Image>();
                    img.color = Color.clear;
                    img.raycastTarget = false;
                }
            }
        }

        // Grid_Content — top-anchored so it grows downward
        GameObject contentGO = GameObject.Find("Grid_Content");
        if (contentGO != null)
        {
            RectTransform crt = contentGO.GetComponent<RectTransform>();
            crt.anchorMin        = new Vector2(0f, 1f);
            crt.anchorMax        = new Vector2(1f, 1f);
            crt.pivot            = new Vector2(0.5f, 1f);
            crt.offsetMin        = Vector2.zero;
            crt.offsetMax        = Vector2.zero;
            crt.anchoredPosition = Vector2.zero;

            // Wire content into ScrollRect if not already set
            if (scrollGO != null)
            {
                ScrollRect sr = scrollGO.GetComponent<ScrollRect>();
                if (sr != null && sr.content == null)
                    sr.content = crt;
            }
        }
    }

    // ── 2. Build / rebuild the button grid ───────────────────────
    private void BuildGrid()
    {
        GameObject content = GameObject.Find("Grid_Content");
        if (content == null) { Debug.LogError("[LevelSelectedBuilder] Grid_Content not found!"); return; }

        foreach (Transform child in content.transform)
            Destroy(child.gameObject);

        // GridLayoutGroup
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>()
            ?? content.AddComponent<GridLayoutGroup>();
        grid.cellSize        = new Vector2(200, 200);
        grid.spacing         = new Vector2(20, 20);
        grid.padding         = new RectOffset(30, 30, 30, 30);
        grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment  = TextAnchor.UpperCenter;

        // ContentSizeFitter — vertical only so width stays screen-wide
        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>()
            ?? content.AddComponent<ContentSizeFitter>();
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        for (int i = 0; i < 15; i++)
        {
            int levelNum    = i + 1;
            int starsEarned = PlayerPrefs.GetInt("Level_" + levelNum + "_Stars", -1);
            var btn         = SpawnButton(content.transform, levelNum, levelNum <= maxUnlocked, starsEarned);

            Transform starsParent = btn.transform.Find("Panel/Stars");
            Transform lockOverlay = btn.transform.Find("Panel/LockOverlay");

            if (levelNum > maxUnlocked)
            {
                // LOCKED: show lock, hide stars
                if (lockOverlay != null) lockOverlay.gameObject.SetActive(true);
                if (starsParent != null) starsParent.gameObject.SetActive(false);
            }
            else if (starsEarned <= 0)
            {
                // UNLOCKED but not played: hide lock, show empty stars
                if (lockOverlay != null) lockOverlay.gameObject.SetActive(false);
                if (starsParent != null)
                {
                    starsParent.gameObject.SetActive(true);
                    ApplyStar(starsParent, "Star1", _sprStarEmpty);
                    ApplyStar(starsParent, "Star2", _sprStarEmpty);
                    ApplyStar(starsParent, "Star3", _sprStarEmpty);
                }
            }
            else
            {
                // COMPLETED: hide lock, show filled stars based on count
                if (lockOverlay != null) lockOverlay.gameObject.SetActive(false);
                if (starsParent != null)
                {
                    starsParent.gameObject.SetActive(true);
                    ApplyStar(starsParent, "Star1", starsEarned >= 1 ? _sprStarFilled : _sprStarEmpty);
                    ApplyStar(starsParent, "Star2", starsEarned >= 2 ? _sprStarFilled : _sprStarEmpty);
                    ApplyStar(starsParent, "Star3", starsEarned >= 3 ? _sprStarFilled : _sprStarEmpty);
                }
            }
        }

        // Force layout rebuild so ContentSizeFitter calculates correct height
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

        Debug.Log("[LevelSelectedBuilder] Built 15 buttons. Unlocked up to: " + maxUnlocked);
    }

    // ── Button factory ────────────────────────────────────────────
    private GameObject SpawnButton(Transform parent, int i, bool unlocked, int stars)
    {
        var btnGO = new GameObject("LvlBtn_" + i);
        btnGO.transform.SetParent(parent, false);

        var bg = btnGO.AddComponent<Image>();
        if (_sprBtn != null)
        {
            bg.sprite = _sprBtn;
            bg.type   = Image.Type.Sliced;
        }
        bg.color = unlocked
            ? new Color(0.10f, 0.35f, 0.85f, 1.00f)
            : new Color(0.05f, 0.10f, 0.25f, 0.85f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = bg;
        var cb = ColorBlock.defaultColorBlock;
        cb.normalColor      = Color.white;
        cb.highlightedColor = new Color(0.80f, 0.90f, 1.00f, 1f);
        cb.pressedColor     = new Color(0.60f, 0.70f, 0.90f, 1f);
        cb.colorMultiplier  = 1f;
        btn.colors = cb;

        var panelGO = MakeRect(btnGO.transform, "Panel");
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

        if (unlocked) BuildUnlockedContent(panelGO.transform, i, stars);
        else          BuildLockedContent(panelGO.transform, i);

        int  levelNum   = i;
        bool isUnlocked = unlocked;
        btn.onClick.AddListener(() =>
        {
            if (!isUnlocked) return;
            PlayerPrefs.SetInt("SelectedLevel", levelNum);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Game");
        });

        return btnGO;
    }

    // ── Unlocked: large number + cyan stars ───────────────────────
    private void BuildUnlockedContent(Transform parent, int level, int stars)
    {
        var numGO  = MakeRect(parent, "Number");
        var numRT  = numGO.GetComponent<RectTransform>();
        numRT.anchorMin = new Vector2(0f, 0.35f);
        numRT.anchorMax = new Vector2(1f, 1.00f);
        numRT.offsetMin = numRT.offsetMax = Vector2.zero;
        var numTmp = numGO.AddComponent<TextMeshProUGUI>();
        numTmp.text      = level.ToString();
        numTmp.fontSize  = 52;
        numTmp.fontStyle = FontStyles.Bold;
        numTmp.color     = Color.white;
        numTmp.alignment = TextAlignmentOptions.Center;

        var starsGO = MakeRect(parent, "Stars");
        var starsRT = starsGO.GetComponent<RectTransform>();
        starsRT.anchorMin = new Vector2(0.05f, 0.00f);
        starsRT.anchorMax = new Vector2(0.95f, 0.38f);
        starsRT.offsetMin = starsRT.offsetMax = Vector2.zero;
        var hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment        = TextAnchor.MiddleCenter;
        hlg.spacing               = 6f;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        for (int s = 1; s <= 3; s++)
        {
            var starGO  = MakeRect(starsGO.transform, "Star" + s);
            starGO.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);
            starGO.AddComponent<Image>().color = Color.white;
        }

        var lockOverlayGO = MakeRect(parent, "LockOverlay");
        lockOverlayGO.SetActive(false);
    }

    // ── Locked: gold lock icon + small number ─────────────────────
    private void BuildLockedContent(Transform parent, int level)
    {
        var lockGO  = MakeRect(parent, "LockOverlay");
        var lockRT  = lockGO.GetComponent<RectTransform>();
        lockRT.anchorMin = new Vector2(0.20f, 0.20f);
        lockRT.anchorMax = new Vector2(0.80f, 0.85f);
        lockRT.offsetMin = lockRT.offsetMax = Vector2.zero;
        var lockImg = lockGO.AddComponent<Image>();
        lockImg.color = new Color(1f, 0.75f, 0.10f, 0.90f);
        if (_sprLock != null) lockImg.sprite = _sprLock;

        var starsGO = MakeRect(parent, "Stars");
        starsGO.SetActive(false);

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

    // ── Header ────────────────────────────────────────────────────
    private void UpdateHeader()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
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

    // ── Background ────────────────────────────────────────────────
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

    // ── Back button ───────────────────────────────────────────────
    public void BackToMainMenu() => SceneManager.LoadScene("MainMenu");

    // ── Utility ───────────────────────────────────────────────────
    private static GameObject MakeRect(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private void ApplyStar(Transform parent, string childName, Sprite sprite)
    {
        var t = parent.Find(childName);
        if (t == null) return;
        var img = t.GetComponent<Image>();
        if (img == null) return;
        img.sprite = sprite;
        img.color  = Color.white;
    }
}
