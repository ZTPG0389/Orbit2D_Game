using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class FixTitleTextMeshPro
{
    [MenuItem("Tools/PausePanel/Fix Title TextMeshPro")]
    static void Run()
    {
        var go = GameObject.Find("TitleImage");
        if (go == null)
        {
            Debug.LogError("[FixTitleTextMeshPro] 'TitleImage' not found in scene.");
            return;
        }

        var img = go.GetComponent<Image>();
        if (img != null)
            Object.DestroyImmediate(img, true);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
            tmp = go.AddComponent<TextMeshProUGUI>();

        tmp.text      = "\U0001F512 PAUSED";
        tmp.fontSize  = 45f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var rt = go.GetComponent<RectTransform>();
        Undo.RecordObject(rt, "Fix Title TextMeshPro");
        rt.anchorMin        = new Vector2(0f,   1f);
        rt.anchorMax        = new Vector2(1f,   1f);
        rt.pivot            = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f,   0f);
        rt.sizeDelta        = new Vector2(0f, 120f);

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);

        Selection.activeGameObject = go;
        Debug.Log("TextMeshPro added to TitleImage!");
    }
}
