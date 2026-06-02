#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class CopyExplosionToResources
{
    const string SourcePath = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Eerie/CFXR2 WW Enemy Explosion.prefab";
    const string DestFolder = "Assets/Resources/Effects";
    const string DestPath   = "Assets/Resources/Effects/CFXR2 WW Enemy Explosion.prefab";

    [MenuItem("Tools/Game/Copy Explosion To Resources")]
    public static void Copy()
    {
        // Verify source exists
        if (!File.Exists(Path.GetFullPath(SourcePath)))
        {
            Debug.LogError($"[CopyExplosion] Source not found: {SourcePath}");
            return;
        }

        // Create Resources/Effects/ if needed
        if (!AssetDatabase.IsValidFolder(DestFolder))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Effects");
            Debug.Log("[CopyExplosion] Created folder: " + DestFolder);
        }

        // Overwrite if already exists
        if (File.Exists(Path.GetFullPath(DestPath)))
            AssetDatabase.DeleteAsset(DestPath);

        bool ok = AssetDatabase.CopyAsset(SourcePath, DestPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (ok)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DestPath);
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"[CopyExplosion] Copied to {DestPath} — Resources.Load(\"Effects/CFXR2 WW Enemy Explosion\") will now work at runtime.");
        }
        else
        {
            Debug.LogError("[CopyExplosion] AssetDatabase.CopyAsset failed.");
        }
    }
}
#endif
