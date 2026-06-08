using UnityEditor;
using UnityEngine;
using System.IO;

public static class BuildScript
{
    public static void BuildAndroid()
    {
        string outputDir  = "Builds/Android";
        string outputPath = Path.Combine(outputDir, "OrbitDrop3D.apk");

        Directory.CreateDirectory(outputDir);

        var options = new BuildPlayerOptions
        {
            scenes      = GetEnabledScenes(),
            locationPathName = outputPath,
            target      = BuildTarget.Android,
            options     = BuildOptions.None
        };

        Debug.Log($"[BuildScript] Starting Android build → {outputPath}");
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"[BuildScript] Build result: {report.summary.result}  " +
                  $"errors={report.summary.totalErrors}  " +
                  $"size={report.summary.totalSize / 1024 / 1024} MB");

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) scenes.Add(s.path);
        return scenes.ToArray();
    }
}
