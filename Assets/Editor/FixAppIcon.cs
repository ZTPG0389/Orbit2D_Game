#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

// Generates AppIcon_Safe.png — a 432x432 black-background icon with the
// original Splash_img scaled to 66% (285x285) and centered, so all content
// stays inside the Android Adaptive Icon safe zone.
public static class FixAppIcon
{
    private const string SourcePath = "Assets/Resources/Sprites/UI/Splash_img.png";
    private const string OutputPath = "Assets/Resources/Sprites/UI/AppIcon_Safe.png";

    private const int CanvasSize  = 432;           // largest adaptive icon slot
    private const int SafeContent = 285;           // 66% of 432 = 285.12 → 285 px
    private const int Margin      = (CanvasSize - SafeContent) / 2;   // 73 px each side

    [InitializeOnLoadMethod]
    static void AutoRun()
    {
        if (SessionState.GetBool("FixAppIcon_done", false)) return;
        SessionState.SetBool("FixAppIcon_done", true);
        EditorApplication.delayCall += Run;
    }

    [MenuItem("OrbitDestroyer/Fix Cropped Icon")]
    public static void Run()
    {
        // ── Step 1: make source readable ────────────────────────────────────
        var srcImporter = AssetImporter.GetAtPath(SourcePath) as TextureImporter;
        if (srcImporter == null)
        {
            Debug.LogError($"[FixAppIcon] TextureImporter not found for '{SourcePath}'.");
            return;
        }

        bool wasReadable = srcImporter.isReadable;
        if (!wasReadable)
        {
            srcImporter.isReadable = true;
            AssetDatabase.ImportAsset(SourcePath, ImportAssetOptions.ForceUpdate);
        }

        var src = AssetDatabase.LoadAssetAtPath<Texture2D>(SourcePath);
        if (src == null)
        {
            Debug.LogError($"[FixAppIcon] Failed to load '{SourcePath}' after enabling isReadable.");
            return;
        }

        // ── Step 2: build 432x432 canvas with black background ──────────────
        var canvas = new Texture2D(CanvasSize, CanvasSize, TextureFormat.RGBA32, false);

        // Fill entire canvas with opaque black
        var black = new Color32[CanvasSize * CanvasSize];
        for (int i = 0; i < black.Length; i++)
            black[i] = new Color32(0, 0, 0, 255);
        canvas.SetPixels32(black);

        // ── Step 3: scale source into safe zone, composite over black ────────
        // Bilinear sampling: map each target pixel back to a UV on the source.
        // Alpha-composite over black so the output is always fully opaque.
        for (int y = 0; y < SafeContent; y++)
        {
            for (int x = 0; x < SafeContent; x++)
            {
                float u = (float)x / (SafeContent - 1);
                float v = (float)y / (SafeContent - 1);
                Color s = src.GetPixelBilinear(u, v);

                // Composite src over black: RGB = src.rgb * src.a + 0 * (1-src.a)
                canvas.SetPixel(Margin + x, Margin + y,
                    new Color(s.r * s.a, s.g * s.a, s.b * s.a, 1f));
            }
        }

        canvas.Apply();

        // ── Step 4: restore source import settings ───────────────────────────
        if (!wasReadable)
        {
            srcImporter.isReadable = false;
            AssetDatabase.ImportAsset(SourcePath, ImportAssetOptions.ForceUpdate);
        }

        // ── Step 5: write PNG to disk ────────────────────────────────────────
        byte[] png = canvas.EncodeToPNG();
        Object.DestroyImmediate(canvas);

        Directory.CreateDirectory(Path.GetDirectoryName(
            Path.GetFullPath(OutputPath)));
        File.WriteAllBytes(OutputPath, png);
        Debug.Log($"[FixAppIcon] Wrote {CanvasSize}x{CanvasSize} icon " +
                  $"(content {SafeContent}x{SafeContent}, margin {Margin}px) → {OutputPath}");

        // ── Step 6: import the new asset as a plain Texture2D ────────────────
        AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);

        var outImporter = AssetImporter.GetAtPath(OutputPath) as TextureImporter;
        if (outImporter != null)
        {
            outImporter.textureType         = TextureImporterType.Default;
            outImporter.isReadable          = false;
            outImporter.mipmapEnabled       = false;
            outImporter.alphaIsTransparency = false;   // output is fully opaque
            outImporter.textureCompression  = TextureImporterCompression.Uncompressed;
            outImporter.maxTextureSize      = 512;     // 432 fits inside 512
            outImporter.npotScale           = TextureImporterNPOTScale.None;

            // Per-platform: keep uncompressed on Android so icon quality is lossless
            var androidSettings = outImporter.GetPlatformTextureSettings("Android");
            androidSettings.overridden        = true;
            androidSettings.maxTextureSize    = 512;
            androidSettings.format            = TextureImporterFormat.RGBA32;
            androidSettings.compressionQuality = 100;
            outImporter.SetPlatformTextureSettings(androidSettings);

            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
        }

        // ── Step 7: load and assign to all Android icon slots ────────────────
        var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
        if (iconTex == null)
        {
            Debug.LogError($"[FixAppIcon] Could not load generated icon from '{OutputPath}'.");
            return;
        }

        SetKind(iconTex, AndroidPlatformIconKind.Adaptive);
        SetKind(iconTex, AndroidPlatformIconKind.Legacy);
        SetKind(iconTex, AndroidPlatformIconKind.Round);

        AssetDatabase.SaveAssets();
        Debug.Log("[FixAppIcon] All Android icon slots updated. " +
                  "Verify: Edit > Project Settings > Player > Android > Icon.");
    }

    static void SetKind(Texture2D tex, PlatformIconKind kind)
    {
        var icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, kind);
        if (icons == null || icons.Length == 0)
        {
            Debug.LogWarning($"[FixAppIcon] No slots for '{kind}'. " +
                             "Switch active platform to Android first " +
                             "(File > Build Settings > Android > Switch Platform).");
            return;
        }
        for (int i = 0; i < icons.Length; i++)
            icons[i].SetTexture(tex);
        PlayerSettings.SetPlatformIcons(BuildTargetGroup.Android, kind, icons);
        Debug.Log($"[FixAppIcon] {kind}: {icons.Length} slot(s) → AppIcon_Safe.");
    }
}
#endif
