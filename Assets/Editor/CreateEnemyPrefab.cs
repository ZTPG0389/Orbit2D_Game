#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class CreateEnemyPrefab
{
    const string PrefabSavePath = "Assets/Prefabs/EnemyShip.prefab";
    const string SpritePath     = "Assets/Resources/Sprites/UI/enemy_ship_red.png";

    [MenuItem("Tools/Game/Create Enemy Prefab")]
    public static void Create()
    {
        // ── root GameObject ───────────────────────────────────────────────────
        var go = new GameObject("EnemyShip");
        go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        // ── SpriteRenderer ────────────────────────────────────────────────────
        var sr     = go.AddComponent<SpriteRenderer>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite != null)
            sr.sprite = sprite;
        else
            Debug.LogWarning($"[CreateEnemyPrefab] Sprite not found at {SpritePath} — assign manually.");
        sr.sortingOrder = 1;

        // ── Rigidbody2D (Kinematic — EnemyShip2D drives position manually) ───
        var rb          = go.AddComponent<Rigidbody2D>();
        rb.bodyType     = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        // ── CircleCollider2D ──────────────────────────────────────────────────
        var col       = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.4f;

        // ── TrailRenderer ─────────────────────────────────────────────────────
        var trail = go.AddComponent<TrailRenderer>();
        trail.startColor         = new Color(1f, 0.3f, 0f, 1f);   // orange
        trail.endColor           = new Color(1f, 0f,   0f, 0f);   // transparent red
        trail.time               = 0.5f;
        trail.startWidth         = 0.15f;
        trail.endWidth           = 0f;
        trail.minVertexDistance  = 0.05f;
        trail.shadowCastingMode  = ShadowCastingMode.Off;
        trail.receiveShadows     = false;

        // Sprites/Default is safe to use via AssetDatabase inside editor scripts
        var trailMat = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites/Default.mat");
        if (trailMat == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null) trailMat = new Material(shader);
        }
        if (trailMat != null) trail.material = trailMat;

        // ── EnemyShip2D script ────────────────────────────────────────────────
        go.AddComponent<EnemyShip2D>();

        // ── Save prefab ───────────────────────────────────────────────────────
        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        bool success;
        var  prefabAsset = PrefabUtility.SaveAsPrefabAsset(go, PrefabSavePath, out success);
        Object.DestroyImmediate(go);

        if (success)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // Ping the new asset in the Project window
            EditorGUIUtility.PingObject(prefabAsset);
            Debug.Log($"[CreateEnemyPrefab] Prefab saved to {PrefabSavePath}");
        }
        else
        {
            Debug.LogError("[CreateEnemyPrefab] Failed to save prefab.");
        }
    }
}
#endif
