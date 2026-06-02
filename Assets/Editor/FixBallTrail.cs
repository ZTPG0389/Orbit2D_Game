#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FixBallTrail
{
    const string PrefabPath = "Assets/Prefabs/OrbiterBall2D.prefab";

    [MenuItem("Tools/Game/Fix Trail Color Now")]
    public static void Run()
    {
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[FixBallTrail] Prefab not found: " + PrefabPath);
            return;
        }

        // Fix TrailRenderer on root and all children
        foreach (var trail in prefab.GetComponentsInChildren<TrailRenderer>(true))
        {
            trail.startColor         = new Color(0f, 0.9f, 1f, 1f);  // cyan
            trail.endColor           = new Color(0f, 0.4f, 1f, 0f);  // transparent blue
            trail.time               = 0.25f;
            trail.startWidth         = 0.12f;
            trail.endWidth           = 0f;
            trail.minVertexDistance  = 0.05f;
            trail.shadowCastingMode  = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows     = false;
            trail.material           = GetSpritesDefaultMaterial(prefab);
            Debug.Log("[FixBallTrail] Fixed TrailRenderer on: " + trail.gameObject.name);
        }

        // Fix any ParticleSystem color to cyan as well
        foreach (var ps in prefab.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0f, 0.9f, 1f, 1f));
            Debug.Log("[FixBallTrail] Fixed ParticleSystem on: " + ps.gameObject.name);
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
        PrefabUtility.UnloadPrefabContents(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Trail color fixed!");
    }

    // Resolves Sprites/Default material using three fallbacks so it works across Unity versions.
    static Material GetSpritesDefaultMaterial(GameObject prefab)
    {
        // 1. Built-in extra resource (works in most Unity versions)
        var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites/Default.mat");
        if (mat != null) return mat;

        // 2. Runtime built-in resource (editor also supports this)
        mat = Resources.GetBuiltinResource<Material>("Sprites/Default.mat");
        if (mat != null) return mat;

        // 3. Search project for any Sprites-Default material
        foreach (string guid in AssetDatabase.FindAssets("t:Material Sprites-Default"))
        {
            mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat != null) return mat;
        }

        // 4. Reuse the SpriteRenderer's material already on this prefab
        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sharedMaterial != null)
        {
            Debug.LogWarning("[FixBallTrail] Sprites/Default not found — reusing SpriteRenderer material.");
            return sr.sharedMaterial;
        }

        // 5. Last resort: create a material from the Sprites/Default shader
        var shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Debug.LogWarning("[FixBallTrail] Creating new material from Sprites/Default shader.");
            return new Material(shader);
        }

        Debug.LogError("[FixBallTrail] Could not resolve any material — trail will remain pink.");
        return null;
    }
}
#endif
