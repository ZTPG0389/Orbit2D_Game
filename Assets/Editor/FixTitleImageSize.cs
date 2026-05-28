using UnityEditor;
using UnityEngine;

public static class FixTitleImageSize
{
    [MenuItem("Tools/PausePanel/Fix Title Image Size")]
    static void Run()
    {
        var go = GameObject.Find("TitleImage");
        if (go == null)
        {
            Debug.LogError("[FixTitleImageSize] 'TitleImage' not found in scene.");
            return;
        }

        var rt = go.GetComponent<RectTransform>();
        Undo.RecordObject(rt, "Fix Title Image Size");
        rt.sizeDelta        = new Vector2(500f, 80f);
        rt.anchoredPosition = new Vector2(0f, -20f);

        EditorUtility.SetDirty(go);
        Debug.Log("Done!");
    }
}
