#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Run once: Tools ▶ OrbitDestroyer ▶ Build Level Select Scene
/// Clears LevelSelected.unity and rebuilds the full Level Select UI hierarchy.
/// </summary>
public static class LevelSelectSceneBuilder
{
    const string ScenePath = "Assets/Scenes/LevelSelected.unity";

    [MenuItem("Tools/OrbitDestroyer/Build Level Select Scene")]
    public static void Build()
    {
        if (!EditorUtility.DisplayDialog("Build Level Select Scene",
                $"This will CLEAR and rebuild:\n{ScenePath}\n\nContinue?", "Yes", "Cancel"))
            return;

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (var go in scene.GetRootGameObjects())
            Object.DestroyImmediate(go);

        // ── EventSystem ───────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ── Canvas ────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Background (full-screen) ──────────────────────────
        var bgGO = MakeStretch(canvasGO.transform, "Background");
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = Color.white;
        bgImg.raycastTarget = false;
        // Assign your background sprite in the Inspector — Color.white lets it show at full brightness

        // Twinkling star dots
        bgGO.AddComponent<BackgroundStars>();

        // ── Header panel (top 200 px) ─────────────────────────
        var headerGO   = new GameObject("Header");
        headerGO.transform.SetParent(canvasGO.transform, false);
        var headerRect = headerGO.AddComponent<RectTransform>();
        headerRect.anchorMin        = new Vector2(0f, 1f);
        headerRect.anchorMax        = new Vector2(1f, 1f);
        headerRect.pivot            = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta        = new Vector2(0f, 200f);

        var headerBgImg = headerGO.AddComponent<Image>();
        headerBgImg.color = new Color(0f, 0f, 0f, 0.35f);
        headerBgImg.raycastTarget = false;

        // "LEVELS" title — left-aligned
        var titleTmp = MakeText(headerGO.transform, "Title_LEVELS");
        AnchorRect(titleTmp.rectTransform,
            new Vector2(0f, 0.08f), new Vector2(0.64f, 0.92f),
            new Vector2(50f, 0f),   Vector2.zero);
        titleTmp.text      = "LEVELS";
        titleTmp.fontSize  = 76f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color     = Color.white;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;

        // Progress "X/15" — right-aligned
        var progTmp = MakeText(headerGO.transform, "Progress_Text");
        AnchorRect(progTmp.rectTransform,
            new Vector2(0.64f, 0.08f), new Vector2(1f, 0.92f),
            Vector2.zero, new Vector2(-40f, 0f));
        progTmp.text      = "0/15";
        progTmp.fontSize  = 40f;
        progTmp.color     = new Color(0.75f, 0.88f, 1f);
        progTmp.alignment = TextAlignmentOptions.MidlineRight;

        // ── Progress bar (60 px, sits above Back button) ──────
        var barRoot   = new GameObject("ProgressBar_Panel");
        barRoot.transform.SetParent(canvasGO.transform, false);
        var barRootRt = barRoot.AddComponent<RectTransform>();
        barRootRt.anchorMin        = new Vector2(0f, 0f);
        barRootRt.anchorMax        = new Vector2(1f, 0f);
        barRootRt.pivot            = new Vector2(0.5f, 0f);
        barRootRt.anchoredPosition = new Vector2(0f, 120f);
        barRootRt.sizeDelta        = new Vector2(0f, 56f);

        var barBgGO   = new GameObject("Bar_BG");
        barBgGO.transform.SetParent(barRoot.transform, false);
        var barBgRt   = barBgGO.AddComponent<RectTransform>();
        barBgRt.anchorMin = new Vector2(0.04f, 0f);
        barBgRt.anchorMax = new Vector2(0.96f, 1f);
        barBgRt.offsetMin = barBgRt.offsetMax = Vector2.zero;
        var barBgImg  = barBgGO.AddComponent<Image>();
        barBgImg.color = new Color(0.10f, 0.10f, 0.25f);

        var barFillGO   = new GameObject("Bar_Fill");
        barFillGO.transform.SetParent(barBgGO.transform, false);
        var barFillRt   = barFillGO.AddComponent<RectTransform>();
        barFillRt.anchorMin = Vector2.zero;
        barFillRt.anchorMax = Vector2.one;
        barFillRt.offsetMin = barFillRt.offsetMax = Vector2.zero;
        var barFillImg  = barFillGO.AddComponent<Image>();
        barFillImg.color      = new Color(0.25f, 0.65f, 1.00f);
        barFillImg.type       = Image.Type.Filled;
        barFillImg.fillMethod = Image.FillMethod.Horizontal;
        barFillImg.fillAmount = 0f;

        // ── Back button (bottom 110 px) ───────────────────────
        var backGO   = new GameObject("BackButton");
        backGO.transform.SetParent(canvasGO.transform, false);
        var backRt   = backGO.AddComponent<RectTransform>();
        backRt.anchorMin        = new Vector2(0f, 0f);
        backRt.anchorMax        = new Vector2(1f, 0f);
        backRt.pivot            = new Vector2(0.5f, 0f);
        backRt.anchoredPosition = Vector2.zero;
        backRt.sizeDelta        = new Vector2(0f, 110f);

        var backImg = backGO.AddComponent<Image>();
        backImg.color = new Color(0.06f, 0.06f, 0.16f);
        var backBtn  = backGO.AddComponent<Button>();

        var backTmp = MakeText(backGO.transform, "Back_Label");
        AnchorRect(backTmp.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        backTmp.text      = "< BACK";
        backTmp.fontSize  = 44f;
        backTmp.fontStyle = FontStyles.Bold;
        backTmp.color     = new Color(0.75f, 0.88f, 1f);
        backTmp.alignment = TextAlignmentOptions.Center;

        // ── ScrollRect for the level grid ─────────────────────
        var scrollGO   = new GameObject("LevelScroll");
        scrollGO.transform.SetParent(canvasGO.transform, false);
        var scrollRt   = scrollGO.AddComponent<RectTransform>();
        // Inset: leave 200 px for header at top, 185 px for bar+back at bottom
        scrollRt.anchorMin = new Vector2(0f, 0f);
        scrollRt.anchorMax = new Vector2(1f, 1f);
        scrollRt.offsetMin = new Vector2(0f, 185f);
        scrollRt.offsetMax = new Vector2(0f, -200f);

        // Transparent backing image (required for raycast in ScrollRect area)
        var scrollImg = scrollGO.AddComponent<Image>();
        scrollImg.color = Color.clear;

        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal        = false;
        scroll.vertical          = true;
        scroll.scrollSensitivity = 35f;
        scroll.decelerationRate  = 0.15f;
        scroll.movementType      = ScrollRect.MovementType.Elastic;
        scroll.elasticity        = 0.1f;

        // Viewport
        var viewGO   = MakeStretch(scrollGO.transform, "Viewport");
        viewGO.AddComponent<RectMask2D>();
        scroll.viewport = viewGO.GetComponent<RectTransform>();

        // Grid content
        var contentGO   = new GameObject("Grid_Content");
        contentGO.transform.SetParent(viewGO.transform, false);
        var contentRt   = contentGO.AddComponent<RectTransform>();
        contentRt.anchorMin        = new Vector2(0f, 1f);
        contentRt.anchorMax        = new Vector2(1f, 1f);
        contentRt.pivot            = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta        = Vector2.zero;
        scroll.content = contentRt;

        // GridLayoutGroup — 3 columns, 180×180 cells, 25 spacing
        var grid = contentGO.AddComponent<GridLayoutGroup>();
        grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.cellSize        = new Vector2(180f, 180f);
        grid.spacing         = new Vector2(25f, 25f);
        grid.padding         = new RectOffset(35, 35, 30, 30);
        grid.childAlignment  = TextAnchor.UpperCenter;

        // ContentSizeFitter auto-expands the content height
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ── LevelSelectManager ────────────────────────────────
        var manager = canvasGO.AddComponent<LevelSelectManager>();
        var so      = new SerializedObject(manager);
        so.FindProperty("gridParent").objectReferenceValue   = contentRt;
        so.FindProperty("progressText").objectReferenceValue = progTmp;
        so.FindProperty("progressFill").objectReferenceValue = barFillImg;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Wire Back button → BackToMainMenu (persistent call)
        UnityEventTools.AddPersistentListener(backBtn.onClick, manager.BackToMainMenu);

        // ── Save ──────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);

        Debug.Log("[LevelSelectSceneBuilder] Done — " + ScenePath);
        EditorUtility.DisplayDialog("Level Select Scene Built!",
            "Scene rebuilt successfully.\n\n" +
            "Remaining steps:\n" +
            "1. Add 'LevelSelected' to Build Settings\n" +
            "2. Add Level1–Level15 scenes to Build Settings\n" +
            "3. (Optional) Assign a nebula sprite to Background Image\n" +
            "4. Wire Main Menu 'Play' button to load 'LevelSelected'",
            "OK");
    }

    // ═══════════════════════════════════════════════════════════
    // MENU: Create stub Level1–Level15 scenes
    // ═══════════════════════════════════════════════════════════
    [MenuItem("Tools/OrbitDestroyer/Create Stub Level Scenes (1–15)")]
    public static void CreateStubScenes()
    {
        if (!EditorUtility.DisplayDialog("Create Stub Scenes",
                "Create empty Level1–Level15 scenes in Assets/Scenes/?\n" +
                "Existing scenes will be skipped.", "Create", "Cancel"))
            return;

        const string folder = "Assets/Scenes";
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        int created = 0;
        for (int i = 1; i <= 15; i++)
        {
            string path = $"{folder}/Level{i}.unity";
            if (File.Exists(path)) continue;

            // NewScene with Additive so we don't lose the current scene
            var s = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(s, path);
            EditorSceneManager.CloseScene(s, true);
            created++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done",
            $"Created {created} stub scene(s) in Assets/Scenes/.\n" +
            "Run 'Add Level Scenes to Build Settings' next.", "OK");
    }

    // ═══════════════════════════════════════════════════════════
    // MENU: Add Level1–Level15 to Build Settings
    // ═══════════════════════════════════════════════════════════
    [MenuItem("Tools/OrbitDestroyer/Add Level Scenes to Build Settings")]
    public static void AddLevelsToBuildSettings()
    {
        var scenes  = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        var added   = new List<string>();
        var missing = new List<string>();

        for (int i = 1; i <= 15; i++)
        {
            string wantedName = $"Level{i}";

            // Search Assets/ for a scene file whose name matches exactly
            string path = null;
            foreach (string guid in AssetDatabase.FindAssets($"t:Scene {wantedName}"))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == wantedName) { path = p; break; }
            }

            if (path == null) { missing.Add(wantedName); continue; }

            // Skip duplicates
            bool already = false;
            foreach (var e in scenes) if (e.path == path) { already = true; break; }
            if (already) continue;

            scenes.Add(new EditorBuildSettingsScene(path, true));
            added.Add(wantedName);
        }

        EditorBuildSettings.scenes = scenes.ToArray();

        string msg = added.Count > 0
            ? $"Added to Build Settings:\n{string.Join(", ", added)}\n\n"
            : "No new scenes were added (all already present or none found).\n\n";

        if (missing.Count > 0)
            msg += $"Missing (create them first):\n{string.Join(", ", missing)}\n\n" +
                   "Run 'Create Stub Level Scenes' to generate placeholder scenes.";

        EditorUtility.DisplayDialog("Build Settings Updated", msg, "OK");
    }

    // ═══════════════════════════════════════════════════════════
    // MENU: Pre-populate 15 level buttons so hierarchy is visible
    //       in Edit Mode — identical to Play Mode structure.
    //       Run once after "Build Level Select Scene".
    // ═══════════════════════════════════════════════════════════
    [MenuItem("Tools/OrbitDestroyer/Populate Level Buttons (Preview)")]
    public static void PopulateLevelButtons()
    {
        const string SpritesPath = "Assets/Resources/Sprites/UI/";

        var contentGO = GameObject.Find("Grid_Content");
        if (contentGO == null)
        {
            EditorUtility.DisplayDialog("Missing Object",
                "'Grid_Content' not found.\nRun 'Build Level Select Scene' first.", "OK");
            return;
        }

        // Load all five UI sprites from the Resources folder
        Sprite glowSpr   = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesPath + "glow_border.png");
        Sprite btnSpr    = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesPath + "level_button.png");
        Sprite lockSpr   = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesPath + "lock_icon.png");
        Sprite filledSpr = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesPath + "star_filled.png");
        Sprite emptySpr  = AssetDatabase.LoadAssetAtPath<Sprite>(SpritesPath + "star_empty.png");

        if (glowSpr == null || btnSpr == null)
        {
            EditorUtility.DisplayDialog("Missing Sprites",
                "UI sprites not found at Assets/Resources/Sprites/UI/.\n" +
                "Run 'Tools > Generate Level Select Sprites' first.", "OK");
            return;
        }

        // Remove any existing LvlBtn_ children
        var toRemove = new List<GameObject>();
        foreach (Transform child in contentGO.transform)
            if (child.name.StartsWith("LvlBtn_")) toRemove.Add(child.gameObject);
        foreach (var go in toRemove) Object.DestroyImmediate(go);

        // Create all 15 buttons in their default locked/preview state
        for (int i = 1; i <= 15; i++)
            BuildPreviewButton(contentGO.transform, i, glowSpr, btnSpr, lockSpr, filledSpr, emptySpr);

        var scene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, scene.path);

        EditorUtility.DisplayDialog("Done",
            "15 level buttons created in Grid_Content.\n\n" +
            "Edit Mode: all buttons visible in locked state.\n" +
            "Play Mode: LevelSelectManager updates state from PlayerPrefs.", "OK");
    }

    static readonly Color _previewGlow  = new Color(0.12f, 0.12f, 0.28f, 0.40f);
    static readonly Color _previewFace  = new Color(0.05f, 0.05f, 0.12f, 1.00f);
    static readonly Color _previewNum   = new Color(1f,    1f,    1f,    0.28f);
    static readonly Color _previewStar  = new Color(0.20f, 0.20f, 0.30f, 0.60f);

    static void BuildPreviewButton(Transform parent, int level,
        Sprite glowSpr, Sprite btnSpr, Sprite lockSpr, Sprite filledSpr, Sprite emptySpr)
    {
        // ── Root ─────────────────────────────────────────────
        var root = new GameObject($"LvlBtn_{level:D2}");
        root.transform.SetParent(parent, false);

        var outerImg = root.AddComponent<Image>();
        outerImg.sprite = glowSpr;
        outerImg.type   = Image.Type.Simple;
        outerImg.color  = _previewGlow;
        outerImg.raycastTarget = true;

        var btn = root.AddComponent<Button>();
        btn.transition   = Selectable.Transition.None;
        btn.interactable = false;   // LevelSelectManager sets this at runtime

        var lb = root.AddComponent<LevelButton>();
        lb.outerGlow = outerImg;

        // ── Panel ─────────────────────────────────────────────
        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(root.transform, false);
        var panelRt = panelGO.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = new Vector2( 4f,  4f);
        panelRt.offsetMax = new Vector2(-4f, -4f);

        var innerImg = panelGO.AddComponent<Image>();
        innerImg.sprite = btnSpr;
        innerImg.type   = Image.Type.Simple;
        innerImg.color  = _previewFace;
        innerImg.raycastTarget = false;
        lb.innerPanel = innerImg;

        // ── Sheen ─────────────────────────────────────────────
        var sheenGO = new GameObject("Sheen");
        sheenGO.transform.SetParent(panelGO.transform, false);
        var sheenRt = sheenGO.AddComponent<RectTransform>();
        sheenRt.anchorMin = new Vector2(0.08f, 0.76f);
        sheenRt.anchorMax = new Vector2(0.92f, 0.94f);
        sheenRt.offsetMin = sheenRt.offsetMax = Vector2.zero;
        var sheenImg = sheenGO.AddComponent<Image>();
        sheenImg.color = new Color(1f, 1f, 1f, 0.03f);
        sheenImg.raycastTarget = false;

        // ── Number ────────────────────────────────────────────
        var numGO = new GameObject("Number");
        numGO.transform.SetParent(panelGO.transform, false);
        var numRt = numGO.AddComponent<RectTransform>();
        numRt.anchorMin = new Vector2(0.05f, 0.36f);
        numRt.anchorMax = new Vector2(0.95f, 0.94f);
        numRt.offsetMin = numRt.offsetMax = Vector2.zero;
        var numTmp = numGO.AddComponent<TextMeshProUGUI>();
        numTmp.text      = level.ToString();
        numTmp.fontSize  = 44f;
        numTmp.fontStyle = FontStyles.Bold;
        numTmp.color     = _previewNum;
        numTmp.alignment = TextAlignmentOptions.Center;
        lb.numberText = numTmp;

        // ── Stars (3 Image children) ──────────────────────────
        var starsGO = new GameObject("Stars");
        starsGO.transform.SetParent(panelGO.transform, false);
        var starsRt = starsGO.AddComponent<RectTransform>();
        starsRt.anchorMin = new Vector2(0.05f, 0.02f);
        starsRt.anchorMax = new Vector2(0.95f, 0.40f);
        starsRt.offsetMin = starsRt.offsetMax = Vector2.zero;

        var hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment         = TextAnchor.MiddleCenter;
        hlg.spacing                = 4f;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        var starImgs = new Image[3];
        for (int s = 0; s < 3; s++)
        {
            var sGO = new GameObject($"Star{s + 1}");
            sGO.transform.SetParent(starsGO.transform, false);
            sGO.AddComponent<RectTransform>().sizeDelta = new Vector2(28f, 28f);
            var sImg = sGO.AddComponent<Image>();
            sImg.sprite       = emptySpr;
            sImg.color        = _previewStar;
            sImg.raycastTarget = false;
            starImgs[s] = sImg;
        }
        lb.F             = starImgs;
        lb.sprStarFilled = filledSpr;
        lb.sprStarEmpty  = emptySpr;

        // ── LockOverlay (all buttons start locked in preview) ─
        var lockRoot = new GameObject("LockOverlay");
        lockRoot.transform.SetParent(panelGO.transform, false);
        var lockRt = lockRoot.AddComponent<RectTransform>();
        lockRt.anchorMin = Vector2.zero;
        lockRt.anchorMax = Vector2.one;
        lockRt.offsetMin = lockRt.offsetMax = Vector2.zero;

        var lockBg = lockRoot.AddComponent<Image>();
        lockBg.color = new Color(0f, 0f, 0f, 0.45f);
        lockBg.raycastTarget = false;

        var iconGO = new GameObject("LockIcon");
        iconGO.transform.SetParent(lockRoot.transform, false);
        var iconRt = iconGO.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.25f, 0.25f);
        iconRt.anchorMax = new Vector2(0.75f, 0.75f);
        iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;

        var iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite       = lockSpr;
        iconImg.color        = Color.white;
        iconImg.raycastTarget = false;

        lb.lockOverlay = lockRoot;
    }

    // ── Helpers ──────────────────────────────────────────────

    /// <summary>Creates a child that stretches to fill its parent.</summary>
    static GameObject MakeStretch(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    /// <summary>Creates a TextMeshProUGUI child with a RectTransform.</summary>
    static TextMeshProUGUI MakeText(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go.AddComponent<TextMeshProUGUI>();
    }

    /// <summary>Sets all four anchor/offset values on a RectTransform.</summary>
    static void AnchorRect(RectTransform rt,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }
}
#endif
