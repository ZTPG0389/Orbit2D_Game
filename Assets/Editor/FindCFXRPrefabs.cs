#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

public static class FindCFXRPrefabs
{
    [MenuItem("Tools/Game/Find CFXR Prefabs")]
    public static void Find()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

        var explosions = new StringBuilder();
        var fire       = new StringBuilder();
        var other      = new StringBuilder();
        int total      = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            string nameLow = name.ToLower();

            if (!nameLow.Contains("cfxr")) continue;
            total++;

            string line = $"  {name}\n    → {path}";
            if (nameLow.Contains("explosion") || nameLow.Contains("burst"))
                explosions.AppendLine(line);
            else if (nameLow.Contains("fire") || nameLow.Contains("flame"))
                fire.AppendLine(line);
            else
                other.AppendLine(line);
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[FindCFXRPrefabs] Found {total} CFXR prefab(s)\n");

        if (explosions.Length > 0)
            sb.AppendLine("── EXPLOSIONS / BURSTS ──────────────────────────────\n" + explosions);
        if (fire.Length > 0)
            sb.AppendLine("── FIRE / FLAME ─────────────────────────────────────\n" + fire);
        if (other.Length > 0)
            sb.AppendLine("── OTHER ─────────────────────────────────────────────\n" + other);

        sb.AppendLine("─────────────────────────────────────────────────────");
        sb.AppendLine("To use one at runtime: copy the prefab into Assets/Resources/Effects/");
        sb.AppendLine("Then Resources.Load<GameObject>(\"Effects/<PrefabName>\") will find it.");
        sb.AppendLine("Or drag it directly into EnemyShip2D.explosionPrefab in the Inspector.");

        Debug.Log(sb.ToString());
    }
}
#endif
