using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;

public static class ProjectHealthCheck
{
    [MenuItem("Tools/Run Project Health Check")]
    public static void Run()
    {
        Debug.Log("=== PROJECT HEALTH CHECK START ===");

        // 1. Missing scripts
        int missingTotal = 0;
        var missingList = new System.Text.StringBuilder();
        foreach (var go in Object.FindObjectsOfType<GameObject>(true))
        {
            int miss = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (miss > 0)
            {
                missingList.Append($" | x{miss} on '{GetPath(go)}'");
                missingTotal += miss;
            }
        }
        Debug.Log($"[CHECK] Missing scripts: {(missingTotal == 0 ? "NONE" : missingTotal.ToString() + missingList.ToString())}");

        // 2. Camera
        var cam = Camera.main;
        Debug.Log($"[CHECK] Camera.main: {(cam != null ? cam.name + " tag=" + cam.tag : "NULL - Camera.main will fail at runtime!")}");
        if (cam != null)
        {
            var al = cam.GetComponent<AudioListener>();
            Debug.Log($"[CHECK] AudioListener on camera: {(al != null ? "OK enabled=" + al.enabled : "MISSING - no audio in build!")}");
        }

        // 3. EnemySpawner
        var es = Object.FindObjectOfType<EnemySpawner>(true);
        if (es != null)
            Debug.Log($"[CHECK] EnemySpawner: on='{es.gameObject.name}' enabled={es.enabled} goActive={es.gameObject.activeInHierarchy}");
        else
            Debug.LogError("[CHECK] EnemySpawner: NOT FOUND in scene!");

        // 4. Resources
        CheckRes("Sprites/UI/enemy_ship_red", typeof(Sprite));
        CheckRes("Effects/CFXR2 WW Explosion", typeof(GameObject));
        CheckRes("Effects/CFXR2 WW Enemy Explosion", typeof(GameObject));
        CheckRes("Sprites/UI/star_filled", typeof(Sprite));
        CheckRes("Sprites/UI/star_empty", typeof(Sprite));
        CheckRes("Sprites/UI/lock_icon", typeof(Sprite));
        CheckRes("Sprites/UI/level_button", typeof(Sprite));
        CheckRes("Sprites/UI/space_bg", typeof(Sprite));

        // 5. Build settings scenes
        Debug.Log("[CHECK] Build Settings scenes:");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            bool exists = File.Exists(path);
            Debug.Log($"  [{i}] {path}  exists={exists}");
        }

        // 6. Key prefabs
        CheckPrefab("Prefabs/OrbiterBall2D");
        CheckPrefab("Prefabs/TargetRing2D");
        CheckPrefab("Prefabs/FloatingScorePopup");
        CheckPrefab("Prefabs/EnemyShip");

        Debug.Log("=== PROJECT HEALTH CHECK END ===");
    }

    static void CheckRes(string path, System.Type type)
    {
        var obj = Resources.Load(path, type);
        if (obj == null)
            Debug.LogError($"[CHECK] Resources.Load MISSING: {path}");
        else
            Debug.Log($"[CHECK] Resources.Load OK: {path}");
    }

    static void CheckPrefab(string assetPath)
    {
        var full = "Assets/" + assetPath + ".prefab";
        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(full);
        Debug.Log($"[CHECK] Prefab '{assetPath}': {(obj != null ? "OK" : "MISSING at " + full)}");
    }

    [MenuItem("Tools/Fix Missing Scripts in Scene")]
    public static void FixMissingScripts()
    {
        int removed = 0;
        foreach (var go in Object.FindObjectsOfType<GameObject>(true))
        {
            int before = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (before > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                Debug.Log($"[FIX] Removed {before} missing script(s) from '{GetPath(go)}'");
                removed += before;
                EditorUtility.SetDirty(go);
            }
        }
        if (removed > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[FIX] Done — removed {removed} missing script(s). Scene saved.");
        }
        else
        {
            Debug.Log("[FIX] No missing scripts found.");
        }
    }

    static string GetPath(GameObject go)
    {
        string p = go.name;
        var t = go.transform.parent;
        while (t != null) { p = t.name + "/" + p; t = t.parent; }
        return p;
    }
}
