using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor.SceneManagement;

public static class VideoSetupEditor
{
    [MenuItem("OrbitDestroyer/Setup Video Background")]
    static void SetupVideoBackground()
    {
        // ── 1. Create RenderTexture asset ─────────────────────────────────────
        const string RT_FOLDER = "Assets/RenderTextures";
        const string RT_PATH   = RT_FOLDER + "/VideoBackground.renderTexture";

        if (!AssetDatabase.IsValidFolder(RT_FOLDER))
            AssetDatabase.CreateFolder("Assets", "RenderTextures");

        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(RT_PATH);
        if (rt == null)
        {
            rt = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32);
            rt.name = "VideoBackground";
            rt.filterMode = FilterMode.Bilinear;
            rt.wrapMode   = TextureWrapMode.Clamp;
            rt.Create();
            AssetDatabase.CreateAsset(rt, RT_PATH);
            AssetDatabase.SaveAssets();
            Debug.Log("[VideoSetup] Created RenderTexture: " + RT_PATH);
        }
        else
        {
            Debug.Log("[VideoSetup] RenderTexture already exists: " + RT_PATH);
        }

        // ── 2. Find Background GameObject ─────────────────────────────────────
        GameObject bgGO = GameObject.Find("Background");
        if (bgGO == null)
        {
            Debug.LogError("[VideoSetup] 'Background' GameObject not found in scene.");
            return;
        }

        // ── 3. Configure VideoPlayer ───────────────────────────────────────────
        VideoPlayer vp = bgGO.GetComponent<VideoPlayer>();
        if (vp == null)
        {
            Debug.LogError("[VideoSetup] 'Background' has no VideoPlayer component.");
            return;
        }

        vp.renderMode      = VideoRenderMode.RenderTexture;
        vp.targetTexture   = rt;
        vp.playOnAwake     = true;
        vp.isLooping       = true;
        vp.waitForFirstFrame = true;
        vp.skipOnDrop      = true;
        Debug.Log("[VideoSetup] VideoPlayer — renderMode=RenderTexture targetTexture=" + rt.name +
                  " clip=" + (vp.clip != null ? vp.clip.name : "NULL"));

        // ── 4. Add RawImage to Background (it already has RectTransform) ──────
        RawImage rawImage = bgGO.GetComponent<RawImage>();
        if (rawImage == null)
        {
            rawImage = bgGO.AddComponent<RawImage>();
            Debug.Log("[VideoSetup] Added RawImage to Background.");
        }
        rawImage.texture = rt;
        rawImage.color   = Color.white;

        // ── 5. Stretch RawImage to fill the full Canvas ────────────────────────
        RectTransform rect = bgGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;    // bottom-left corner
        rect.anchorMax = Vector2.one;     // top-right corner
        rect.offsetMin = Vector2.zero;    // left/bottom padding = 0
        rect.offsetMax = Vector2.zero;    // right/top padding = 0
        Debug.Log("[VideoSetup] RectTransform set to full-screen stretch.");

        // ── 6. Put Background behind all other UI elements ─────────────────────
        bgGO.transform.SetSiblingIndex(0);
        Debug.Log("[VideoSetup] Background sibling index set to 0 (behind all UI).");

        // ── 7. Add VideoPlayerDebug script ─────────────────────────────────────
        if (bgGO.GetComponent<VideoPlayerDebug>() == null)
        {
            bgGO.AddComponent<VideoPlayerDebug>();
            Debug.Log("[VideoSetup] Added VideoPlayerDebug component.");
        }

        // ── 8. Android video clip compatibility ────────────────────────────────
        if (vp.clip != null)
        {
            string clipPath = AssetDatabase.GetAssetPath(vp.clip);
            VideoClipImporter importer = AssetImporter.GetAtPath(clipPath) as VideoClipImporter;
            if (importer != null)
            {
                // Default platform settings
                VideoImporterTargetSettings defaults = importer.defaultTargetSettings;
                defaults.enableTranscoding  = true;
                defaults.codec              = VideoCodec.H264;
                defaults.bitrateMode        = VideoBitrateMode.High;
                defaults.spatialQuality     = VideoSpatialQuality.HighSpatialQuality;
                importer.defaultTargetSettings = defaults;

                // Android-specific settings — GetTargetSettings returns null when no
                // Android override has been saved yet, so create a new instance in that case.
                VideoImporterTargetSettings android =
                    importer.GetTargetSettings("Android") ?? new VideoImporterTargetSettings();
                android.enableTranscoding = true;
                android.codec             = VideoCodec.H264;
                android.bitrateMode       = VideoBitrateMode.High;
                android.spatialQuality    = VideoSpatialQuality.HighSpatialQuality;
                importer.SetTargetSettings("Android", android);

                importer.SaveAndReimport();
                Debug.Log("[VideoSetup] Android clip settings applied — H264, High bitrate: " + clipPath);
            }
        }
        else
        {
            Debug.LogWarning("[VideoSetup] VideoPlayer has no clip assigned. Assign one in the Inspector.");
        }

        // ── 9. Save scene ──────────────────────────────────────────────────────
        EditorUtility.SetDirty(bgGO);
        EditorSceneManager.MarkSceneDirty(bgGO.scene);
        EditorSceneManager.SaveScene(bgGO.scene);

        Debug.Log("[VideoSetup] ✓ Setup complete — press Play to verify video playback.");
    }
}
