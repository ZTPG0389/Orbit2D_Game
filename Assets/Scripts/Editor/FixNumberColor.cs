using UnityEngine;
using UnityEditor;

public static class FixNumberColor
{
    [MenuItem("OrbitDestroyer/Fix Number Text Color")]
    static void Fix()
    {
        using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/LvlBtn_01.prefab"))
        {
            var num = scope.prefabContentsRoot.transform.Find("Panel/Number");
            if (num == null) { Debug.LogError("[FixNumber] Panel/Number not found."); return; }
            var tmp = num.GetComponent<TMPro.TMP_Text>();
            if (tmp == null) { Debug.LogError("[FixNumber] No TMP_Text on Number."); return; }
            Color old = tmp.color;
            tmp.color = Color.white;
            Debug.Log($"[FixNumber] Number.color: {old} -> {tmp.color}  Prefab saved.");
        }
    }
}
