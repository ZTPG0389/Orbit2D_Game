#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuilder
{
    const string OutputPath = "Builds/Android/OrbitDrop3D.apk";

    // ── Dev build: Mono backend — fast, low disk use ──────────
    // Use this during development. Skips IL2CPP compilation entirely.
    [MenuItem("Tools/Android/[DEV] Build and Run (Mono — Fast)")]
    public static void BuildAndRunMono()
    {
        if (!SwitchToAndroid()) return;
        SetScriptingBackend(ScriptingImplementation.Mono2x);

        var scenes = GetEnabledScenes();
        if (scenes.Count == 0) { NoScenesDialog(); return; }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes.ToArray(),
            locationPathName = OutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.AutoRunPlayer | BuildOptions.Development
        });

        LogReport(report, "Mono");
    }

    // ── Release build: IL2CPP — optimised, needs 12+ GB free ──
    [MenuItem("Tools/Android/[RELEASE] Build and Run (IL2CPP)")]
    public static void BuildAndRunIL2CPP()
    {
        if (!SwitchToAndroid()) return;

        // Warn if disk is tight
        var drive = new System.IO.DriveInfo("C");
        double freeGB = drive.AvailableFreeSpace / 1073741824.0;
        if (freeGB < 12.0)
        {
            bool proceed = EditorUtility.DisplayDialog("Low Disk Space",
                $"C: drive has only {freeGB:F1} GB free.\n" +
                "IL2CPP needs ~12 GB to compile.\n\n" +
                "Use [DEV] Mono build instead, or free up space first.",
                "Build Anyway", "Cancel");
            if (!proceed) return;
        }

        SetScriptingBackend(ScriptingImplementation.IL2CPP);

        var scenes = GetEnabledScenes();
        if (scenes.Count == 0) { NoScenesDialog(); return; }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes.ToArray(),
            locationPathName = OutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.AutoRunPlayer
        });

        LogReport(report, "IL2CPP");
    }

    // ── Build APK only, no auto-run ────────────────────────────
    [MenuItem("Tools/Android/Build APK Only (Mono)")]
    public static void BuildOnlyMono()
    {
        if (!SwitchToAndroid()) return;
        SetScriptingBackend(ScriptingImplementation.Mono2x);

        var scenes = GetEnabledScenes();
        if (scenes.Count == 0) { NoScenesDialog(); return; }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes.ToArray(),
            locationPathName = OutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.Development
        });

        LogReport(report, "Mono");
    }

    // ── Batch-mode Mono entry point (dev builds, low disk) ───
    public static void BuildAndroidMono()
    {
        Debug.Log("[AndroidBuilder] Batch build started — Mono ARM64");

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        ApplyAndroidSettings(ScriptingImplementation.Mono2x);

        var scenes = GetEnabledScenes();
        if (scenes.Count == 0)
        {
            Debug.LogError("[AndroidBuilder] No enabled scenes — aborting.");
            EditorApplication.Exit(1); return;
        }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes.ToArray(),
            locationPathName = OutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.Development
        });

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[AndroidBuilder] SUCCESS — {report.summary.totalSize/1048576.0:F1} MB → {OutputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[AndroidBuilder] FAILED — errors: {report.summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }

    // ── Batch-mode IL2CPP entry point (called via -executeMethod) ────
    public static void BuildAndroid()
    {
        Debug.Log("[AndroidBuilder] Batch build started — IL2CPP ARM64");

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        ApplyAndroidSettings(ScriptingImplementation.IL2CPP);

        var scenes = GetEnabledScenes();
        if (scenes.Count == 0)
        {
            Debug.LogError("[AndroidBuilder] No enabled scenes — aborting.");
            EditorApplication.Exit(1); return;
        }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes.ToArray(),
            locationPathName = OutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.None
        });

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[AndroidBuilder] SUCCESS — {report.summary.totalSize/1048576.0:F1} MB → {OutputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[AndroidBuilder] FAILED — errors: {report.summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }

    // ── Helpers ───────────────────────────────────────────────

    static void ApplyAndroidSettings(ScriptingImplementation backend)
    {
        // Scripting backend
        if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != backend)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, backend);
            Debug.Log($"[AndroidBuilder] Backend → {backend}");
        }

        // ARM64 + ARMv7 for broadest device support (Android 8+)
        var arch = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        if (PlayerSettings.Android.targetArchitectures != arch)
        {
            PlayerSettings.Android.targetArchitectures = arch;
            Debug.Log($"[AndroidBuilder] Architecture → ARM64 | ARMv7");
        }

        // OpenGLES3 only — remove Vulkan to avoid driver issues on some devices
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
            new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        Debug.Log("[AndroidBuilder] Graphics API → OpenGLES3 only");

        // API levels
        PlayerSettings.Android.minSdkVersion      = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion   = AndroidSdkVersions.AndroidApiLevelAuto;
    }

    static void SetScriptingBackend(ScriptingImplementation backend)
    {
        ApplyAndroidSettings(backend);
    }

    static bool SwitchToAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            return true;

        bool ok = EditorUtility.DisplayDialog("Switch Platform",
            "Active platform is not Android.\nSwitch to Android now?\n\n" +
            "(This may take a minute to reimport assets.)", "Switch", "Cancel");
        if (!ok) return false;

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        return true;
    }

    static List<string> GetEnabledScenes()
    {
        var list = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) list.Add(s.path);
        return list;
    }

    static void NoScenesDialog() =>
        EditorUtility.DisplayDialog("No Scenes",
            "Add scenes to File > Build Settings before building.", "OK");

    static void LogReport(BuildReport report, string backend)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            double mb = report.summary.totalSize / 1048576.0;
            Debug.Log($"[AndroidBuilder] {backend} build succeeded — {mb:F1} MB → {OutputPath}");
            EditorUtility.DisplayDialog("Build Succeeded",
                $"APK built successfully ({backend}).\n\nSize: {mb:F1} MB\nPath: {OutputPath}", "OK");
        }
        else
        {
            Debug.LogError($"[AndroidBuilder] {backend} build FAILED — " +
                           $"{report.summary.result} | Errors: {report.summary.totalErrors}");
            EditorUtility.DisplayDialog("Build Failed",
                $"{backend} build failed with {report.summary.totalErrors} error(s).\n" +
                "Check the Console for details.", "OK");
        }
    }
}
#endif
