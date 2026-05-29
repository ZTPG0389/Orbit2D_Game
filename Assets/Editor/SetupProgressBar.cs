using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public static class SetupProgressBar
{
    [MenuItem("Tools/Levels/Setup Progress Bar")]
    static void Run()
    {
        // ── 1. Find ProgressBar_Panel ─────────────────────────────────────────
        var panel = GameObject.Find("ProgressBar_Panel");
        if (panel == null)
        {
            Debug.LogError("[SetupProgressBar] 'ProgressBar_Panel' not found in scene.");
            return;
        }

        // ── 2. Remove DebugUIHandlerProgressBar if present ────────────────────
        foreach (var comp in panel.GetComponents<Component>())
        {
            if (comp != null && comp.GetType().Name == "DebugUIHandlerProgressBar")
            {
                UnityEngine.Object.DestroyImmediate(comp, true);
                Debug.Log("[SetupProgressBar] Removed DebugUIHandlerProgressBar.");
                break;
            }
        }

        // ── 3. Add ProgressBarUpdater if missing ──────────────────────────────
        Type updaterType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("ProgressBarUpdater"))
            .FirstOrDefault(t => t != null);

        if (updaterType == null)
        {
            Debug.LogError("[SetupProgressBar] 'ProgressBarUpdater' script not found. " +
                           "Create ProgressBarUpdater.cs first, then re-run this tool.");
            return;
        }

        var updater = (Component)(panel.GetComponent(updaterType)
                   ?? panel.AddComponent(updaterType));

        // ── 4. Find Bar_BG/Bar_Fill ───────────────────────────────────────────
        var barFillTransform = panel.transform.Find("Bar_BG/Bar_Fill");
        if (barFillTransform == null)
        {
            Debug.LogWarning("[SetupProgressBar] 'Bar_BG/Bar_Fill' not found — skipping barFill assignment.");
        }

        // ── 5. Find Progress_Text inside Header ───────────────────────────────
        Transform progressTextTransform = null;
        var header = panel.transform.Find("Header");
        if (header != null)
            progressTextTransform = header.Find("Progress_Text");

        if (progressTextTransform == null)
            Debug.LogWarning("[SetupProgressBar] 'Header/Progress_Text' not found — skipping progressText assignment.");

        // ── 6. Assign fields via SerializedObject (handles any access modifier)
        var so = new SerializedObject(updater);

        if (barFillTransform != null)
        {
            var barFillProp = so.FindProperty("barFill");
            if (barFillProp != null)
                barFillProp.objectReferenceValue = barFillTransform.GetComponent<Image>();
            else
                Debug.LogWarning("[SetupProgressBar] 'barFill' field not found on ProgressBarUpdater.");
        }

        if (progressTextTransform != null)
        {
            var textProp = so.FindProperty("progressText");
            if (textProp != null)
                textProp.objectReferenceValue = progressTextTransform.GetComponent<TextMeshProUGUI>();
            else
                Debug.LogWarning("[SetupProgressBar] 'progressText' field not found on ProgressBarUpdater.");
        }

        var totalLevelsProp = so.FindProperty("totalLevels");
        if (totalLevelsProp != null)
            totalLevelsProp.intValue = 15;
        else
            Debug.LogWarning("[SetupProgressBar] 'totalLevels' field not found on ProgressBarUpdater.");

        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(panel);
        EditorSceneManager.MarkSceneDirty(panel.scene);

        Selection.activeGameObject = panel;
        Debug.Log("Progress Bar setup done!");
    }

    [MenuItem("Tools/Levels/Set Progress 8 levels")]
    static void SetProgress8()
    {
        PlayerPrefs.SetInt("MaxUnlockedLevel", 8);
        PlayerPrefs.Save();
        Debug.Log("[SetupProgressBar] MaxUnlockedLevel set to 8.");
    }
}
