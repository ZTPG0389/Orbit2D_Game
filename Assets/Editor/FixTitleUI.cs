using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FixTitleUI
{
    const string TitlePath = "Game/HUD Canvas/PausePanel/Card/Title";

    [MenuItem("Tools/PausePanel/Fix Title Image")]
    static void FixTitleImage()
    {
        var title = GameObject.Find(TitlePath);

        if (title == null)
        {
            Debug.LogError($"[FixTitleUI] GameObject not found at path: {TitlePath}\n" +
                           "Make sure the Game scene is open and the hierarchy matches.");
            return;
        }

        // ── 1. Remove TextMeshProUGUI ────────────────────────────────────────
        var tmp = title.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            Undo.DestroyObjectImmediate(tmp);
            Debug.Log("[FixTitleUI] Removed TextMeshProUGUI component.");
        }
        else
        {
            Debug.Log("[FixTitleUI] No TextMeshProUGUI found — skipping removal.");
        }

        // ── 2. Add Image component (or reuse if already present) ─────────────
        var image = title.GetComponent<Image>();
        if (image == null)
        {
            image = Undo.AddComponent<Image>(title);
            Debug.Log("[FixTitleUI] Added Image component.");
        }
        else
        {
            Debug.Log("[FixTitleUI] Image component already present — keeping it.");
        }

        // ── 3. Set RectTransform ─────────────────────────────────────────────
        var rt = title.GetComponent<RectTransform>();
        Undo.RecordObject(rt, "Fix Title RectTransform");

        rt.anchorMin        = new Vector2(0f,   1f);
        rt.anchorMax        = new Vector2(1f,   1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f,  -10f);
        rt.sizeDelta        = new Vector2(500f, 120f);

        // ── 4. Mark scene dirty and select the object ────────────────────────
        EditorUtility.SetDirty(title);
        EditorSceneManager.MarkSceneDirty(title.scene);

        Selection.activeGameObject = title;

        Debug.Log("[FixTitleUI] RectTransform updated:\n" +
                  $"  anchorMin=(0,1)  anchorMax=(1,1)  pivot=(0.5,1)\n" +
                  $"  anchoredPosition=(0,-10)  sizeDelta=(500,120)\n\n" +
                  "Now assign your Title sprite PNG to the Image component.");
    }
}
