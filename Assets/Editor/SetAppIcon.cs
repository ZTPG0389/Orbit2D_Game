#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

public static class SetAppIcon
{
    private const string TexPath = "Assets/Resources/Sprites/UI/Splash_img.png";

    [InitializeOnLoadMethod]
    static void AutoRun()
    {
        if (SessionState.GetBool("SetAppIcon_done", false)) return;
        SessionState.SetBool("SetAppIcon_done", true);
        EditorApplication.delayCall += Run;
    }

    [MenuItem("OrbitDestroyer/Setup App Icon")]
    public static void Run()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath);
        if (tex == null)
        {
            Debug.LogError($"[SetAppIcon] Texture not found at '{TexPath}'.");
            return;
        }

        // Unity 6 API: SetPlatformIcons(group, kind, icons)
        SetKind(tex, AndroidPlatformIconKind.Adaptive);
        SetKind(tex, AndroidPlatformIconKind.Legacy);
        SetKind(tex, AndroidPlatformIconKind.Round);

        AssetDatabase.SaveAssets();
        Debug.Log("[SetAppIcon] Android Adaptive + Legacy + Round icons set to 'Splash_img'. " +
                  "Verify: Edit > Project Settings > Player > Android > Icon.");
    }

    static void SetKind(Texture2D tex, PlatformIconKind kind)
    {
        var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, kind);
        if (icons == null || icons.Length == 0)
        {
            Debug.LogWarning($"[SetAppIcon] No slots for '{kind}' — " +
                             "switch active platform to Android first.");
            return;
        }
        for (int i = 0; i < icons.Length; i++)
            icons[i].SetTexture(tex);
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, kind, icons);
        Debug.Log($"[SetAppIcon] {kind}: {icons.Length} slot(s) assigned.");
    }
}
#endif
