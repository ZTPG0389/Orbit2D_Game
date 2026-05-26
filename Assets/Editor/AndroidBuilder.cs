#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class AndroidBuilder
{
    const string OutputPath = "Builds/Android/OrbitDrop3D.apk";

    // ── Build + install + launch on connected device ───────────
    [MenuItem("Tools/Android/Build and Run on Device")]
    public static void BuildAndRun()
    {
        if (!SwitchToAndroid()) return;

        var scenes = GetEnabledScenes();
        if (scenes.Count == 0)
        {
            EditorUtility.DisplayDialog("No Scenes",
                "Add scenes to File > Build Settings before building.", "OK");
            return;
        }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes.ToArray(),
            locationPathName = OutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.AutoRunPlayer
        });

        LogReport(report);
    }

    // ── Build APK only (no auto-run) ───────────────────────────
    [MenuItem("Tools/Android/Build APK Only")]
    public static void BuildOnly()
    {
        if (!SwitchToAndroid()) return;

        var scenes = GetEnabledScenes();
        if (scenes.Count == 0)
        {
            EditorUtility.DisplayDialog("No Scenes",
                "Add scenes to File > Build Settings before building.", "OK");
            return;
        }

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes           = scenes.ToArray(),
            locationPathName = OutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.None
        });

        LogReport(report);
    }

    // ── Helpers ───────────────────────────────────────────────

    static bool SwitchToAndroid()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            return true;

        bool ok = EditorUtility.DisplayDialog("Switch Platform",
            "Active platform is not Android.\nSwitch to Android now?\n\n" +
            "(This may take a minute to reimport assets.)", "Switch", "Cancel");

        if (!ok) return false;

        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Android, BuildTarget.Android);
        return true;
    }

    static List<string> GetEnabledScenes()
    {
        var list = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) list.Add(s.path);
        return list;
    }

    static void LogReport(BuildReport report)
    {
        if (report.summary.result == BuildResult.Succeeded)
        {
            double mb = report.summary.totalSize / 1048576.0;
            Debug.Log($"[AndroidBuilder] Build succeeded — {mb:F1} MB → {OutputPath}");
            EditorUtility.DisplayDialog("Build Succeeded",
                $"APK built successfully.\n\nSize: {mb:F1} MB\nPath: {OutputPath}", "OK");
        }
        else
        {
            Debug.LogError($"[AndroidBuilder] Build FAILED — {report.summary.result}\n" +
                           $"Errors: {report.summary.totalErrors}");
            EditorUtility.DisplayDialog("Build Failed",
                $"Build failed with {report.summary.totalErrors} error(s).\n" +
                "Check the Console for details.", "OK");
        }
    }
}
#endif
