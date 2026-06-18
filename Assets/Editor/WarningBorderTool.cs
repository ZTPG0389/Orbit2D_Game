#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class WarningBorderTool
{
    [MenuItem("Tools/UI/Add Border Glow To Warning Panel")]
    public static void AddBorderGlow()
    {
        // ── Find EnemyWarning (works even if the GO is currently inactive) ───────
        var all = Resources.FindObjectsOfTypeAll<EnemyWarning>();

        if (all == null || all.Length == 0)
        {
            EditorUtility.DisplayDialog("EnemyWarning Not Found",
                "No EnemyWarning component found in the scene.\n\n" +
                "Make sure the Game scene is open and the HUD Canvas is present.",
                "OK");
            return;
        }

        var ew = all[0].gameObject;

        // ── Guard: warn if already added ────────────────────────────────────────
        var existing = ew.GetComponent<WarningBorderGlow>();
        if (existing != null)
        {
            bool replace = EditorUtility.DisplayDialog("Already Added",
                $"WarningBorderGlow already exists on '{ew.name}'.\n\nReplace it?",
                "Replace", "Cancel");
            if (!replace) return;

            Undo.DestroyObjectImmediate(existing);
            var existingOutline = ew.GetComponent<Outline>();
            if (existingOutline != null)
                Undo.DestroyObjectImmediate(existingOutline);
        }

        // ── Add WarningBorderGlow ────────────────────────────────────────────────
        // [RequireComponent(Outline)] causes Unity to auto-add Outline first.
        Undo.AddComponent<WarningBorderGlow>(ew);

        // Configure the Outline that [RequireComponent] just added
        var outline = ew.GetComponent<Outline>();
        if (outline != null)
        {
            Undo.RecordObject(outline, "Configure Warning Outline");
            outline.effectColor     = new Color(1f, 0f, 0f, 0.8f);
            outline.effectDistance  = new Vector2(5f, -5f);
            outline.useGraphicAlpha = false;
        }

        // ── Mark dirty and save ──────────────────────────────────────────────────
        EditorUtility.SetDirty(ew);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        // ── Select in Hierarchy so Inspector is visible ──────────────────────────
        Selection.activeGameObject = ew;
        EditorGUIUtility.PingObject(ew);

        // ── Confirmation dialog with Inspector settings ──────────────────────────
        EditorUtility.DisplayDialog("Border Glow Added",
            $"WarningBorderGlow successfully added to '{ew.name}'.\n\n" +
            "─── Inspector Settings ───\n\n" +
            "WarningBorderGlow\n" +
            "  Border Color    :  R=1  G=0  B=0  A=1  (pure red)\n" +
            "  Effect Distance :  X=5  Y=-5\n" +
            "  Min Alpha       :  0.30\n" +
            "  Max Alpha       :  1.00\n" +
            "  Pulse Speed     :  2.5  (≈ 0.4 s / cycle)\n\n" +
            "Outline  (auto-added by [RequireComponent])\n" +
            "  Effect Color    :  R=1  G=0  B=0  A=0.8\n" +
            "  Effect Distance :  X=5  Y=-5\n" +
            "  Use Graphic Alpha : OFF\n\n" +
            "The border pulses red and syncs with EnemyWarning's flash.\n" +
            "No extra wiring needed — it works automatically.",
            "OK");
    }
}
#endif
