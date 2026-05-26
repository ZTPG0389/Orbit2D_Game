using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class LevelSelectedSetup
{
    [MenuItem("Tools/Generate Level Buttons")]
    static void GenerateButtons()
    {
        GameObject content = GameObject.Find("Grid_Content");
        if (content == null)
        {
            Debug.LogError("Grid_Content not found! Open LevelSelected scene first.");
            return;
        }

        // Clear existing buttons
        for (int i = content.transform.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(content.transform.GetChild(i).gameObject);

        // GridLayoutGroup
        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160, 160);
        grid.spacing = new Vector2(15, 15);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 1; i <= 15; i++)
        {
            bool unlocked = (i <= 3); // default first 3

            GameObject btnGO = new GameObject("LvlBtn_" + i);
            Undo.RegisterCreatedObjectUndo(btnGO, "Create Level Button");
            btnGO.transform.SetParent(content.transform, false);

            Image bgImg = btnGO.AddComponent<Image>();
            bgImg.color = unlocked
                ? new Color(0.15f, 0.35f, 0.85f, 1f)
                : new Color(0.08f, 0.08f, 0.25f, 0.9f);

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = bgImg;
            ColorBlock cb = ColorBlock.defaultColorBlock;
            cb.normalColor = Color.white;
            cb.colorMultiplier = 1f;
            btn.colors = cb;

            // Number
            GameObject numGO = new GameObject("Number");
            numGO.transform.SetParent(btnGO.transform, false);
            RectTransform numRT = numGO.AddComponent<RectTransform>();
            numRT.anchorMin = new Vector2(0, 0.3f);
            numRT.anchorMax = Vector2.one;
            numRT.offsetMin = Vector2.zero;
            numRT.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = numGO.AddComponent<TextMeshProUGUI>();
            tmp.text = i.ToString();
            tmp.fontSize = 40;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = unlocked ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;

            // Stars
            GameObject starsGO = new GameObject("Stars");
            starsGO.transform.SetParent(btnGO.transform, false);
            RectTransform srt = starsGO.AddComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.1f, 0f);
            srt.anchorMax = new Vector2(0.9f, 0.35f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            HorizontalLayoutGroup hlg = starsGO.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            for (int s = 1; s <= 3; s++)
            {
                GameObject starGO = new GameObject("Star" + s);
                starGO.transform.SetParent(starsGO.transform, false);
                RectTransform starRT = starGO.AddComponent<RectTransform>();
                starRT.sizeDelta = new Vector2(25, 25);
                Image starImg = starGO.AddComponent<Image>();
                starImg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }

            // Lock
            if (!unlocked)
            {
                GameObject lockGO = new GameObject("Lock");
                lockGO.transform.SetParent(btnGO.transform, false);
                RectTransform lrt = lockGO.AddComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0.25f, 0.25f);
                lrt.anchorMax = new Vector2(0.75f, 0.75f);
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
                Image lockImg = lockGO.AddComponent<Image>();
                lockImg.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
            }
        }

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        Debug.Log("Generated 15 level buttons in Edit mode!");

        // Disable LevelSelectedBuilder to avoid regenerating in Play mode
        var builder = Object.FindObjectOfType<LevelSelectedBuilder>();
        if (builder != null)
        {
            builder.enabled = false;
            Debug.Log("Disabled LevelSelectedBuilder runtime script.");
        }
    }
}
