#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class FixEnemySprites
{
    [MenuItem("Tools/Game/Fix Enemy Sprite PPU")]
    public static void FixPPU()
    {
        string[] guids = AssetDatabase.FindAssets("enemy_ t:Texture2D", new[] { "Assets/Resources/Sprites/UI" });
        if (guids.Length == 0)
        {
            Debug.LogWarning("[FixEnemySprites] No enemy_ sprites found in Assets/Resources/Sprites/UI");
            return;
        }

        int fixed_ = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            if (importer.spritePixelsPerUnit != 100)
            {
                importer.spritePixelsPerUnit = 100;
                importer.textureType         = TextureImporterType.Sprite;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                Debug.Log($"[FixEnemySprites] Set PPU=100 on {path}");
                fixed_++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FixEnemySprites] Done — {fixed_} sprite(s) updated.");
    }
}
#endif
