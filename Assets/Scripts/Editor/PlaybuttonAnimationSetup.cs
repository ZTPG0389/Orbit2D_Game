using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

/// One-shot setup: OrbitDrop3D → Setup Playbutton Premium Animation
/// Creates ButtonGlow child, AnimationClip (idle + glow pulse), AnimatorController,
/// attaches Animator + PlaybuttonAnimator to Playbutton, saves scene.
public static class PlaybuttonAnimationSetup
{
    const string ANIM_FOLDER     = "Assets/Animations";
    const string CLIP_PATH       = ANIM_FOLDER + "/PlaybuttonIdle.anim";
    const string CONTROLLER_PATH = ANIM_FOLDER + "/PlaybuttonPulse.controller";

    [MenuItem("OrbitDrop3D/Setup Playbutton Premium Animation")]
    static void Setup()
    {
        // ── 1. Find Playbutton ────────────────────────────────────────────────
        var pb = GameObject.Find("Playbutton");
        if (pb == null) { Debug.LogError("[PlaybuttonSetup] 'Playbutton' not found."); return; }

        pb.transform.localScale = Vector3.one;

        // ── 2. Create ButtonGlow child ────────────────────────────────────────
        var glowTf = pb.transform.Find("ButtonGlow");
        if (glowTf == null)
        {
            var glowGO        = new GameObject("ButtonGlow");
            glowGO.transform.SetParent(pb.transform, false);
            glowGO.transform.SetSiblingIndex(0);            // behind button Image

            var img           = glowGO.AddComponent<Image>();
            img.color         = new Color(1f, 0.82f, 0.15f, 0f); // gold, starts invisible
            img.raycastTarget = false;                      // never blocks button clicks

            var rect          = glowGO.GetComponent<RectTransform>();
            rect.anchorMin    = Vector2.zero;
            rect.anchorMax    = Vector2.one;
            rect.offsetMin    = new Vector2(-20f, -20f);    // 20px halo on every side
            rect.offsetMax    = new Vector2( 20f,  20f);

            glowTf = glowGO.transform;
            Debug.Log("[PlaybuttonSetup] Created ButtonGlow child.");
        }
        else
        {
            var img = glowTf.GetComponent<Image>();
            if (img) img.raycastTarget = false;
            Debug.Log("[PlaybuttonSetup] Reusing existing ButtonGlow child.");
        }

        // ── 3. Animations folder ──────────────────────────────────────────────
        if (!AssetDatabase.IsValidFolder(ANIM_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Animations");

        // ── 4. Build AnimationClip ────────────────────────────────────────────
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(CLIP_PATH) != null)
            AssetDatabase.DeleteAsset(CLIP_PATH);

        var clip       = new AnimationClip { name = "PlaybuttonIdle", frameRate = 60f };
        float dur      = 1.0f;

        // -- 4a. Button idle scale pulse: 1.0 → 1.08 → 1.0 -------------------
        var btnScale   = BuildSineCurve(1.00f, 1.08f, dur);
        clip.SetCurve("", typeof(RectTransform), "localScale.x", btnScale);
        clip.SetCurve("", typeof(RectTransform), "localScale.y", btnScale);

        // -- 4b. Glow scale pulse: 1.0 → 1.18 → 1.0 (expands further) --------
        var glowScale  = BuildSineCurve(1.00f, 1.18f, dur);
        clip.SetCurve("ButtonGlow", typeof(RectTransform), "localScale.x", glowScale);
        clip.SetCurve("ButtonGlow", typeof(RectTransform), "localScale.y", glowScale);

        // -- 4c. Glow alpha pulse: 0 → 0.55 → 0 ------------------------------
        var glowAlpha  = BuildSineCurve(0f, 0.55f, dur);
        clip.SetCurve("ButtonGlow", typeof(Image), "m_Color.a", glowAlpha);

        // Mark clip as looping
        var settings        = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime   = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, CLIP_PATH);
        AssetDatabase.SaveAssets();
        Debug.Log("[PlaybuttonSetup] AnimationClip saved: " + CLIP_PATH);

        // ── 5. Build AnimatorController ───────────────────────────────────────
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH) != null)
            AssetDatabase.DeleteAsset(CONTROLLER_PATH);

        var controller     = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
        var rootSM         = controller.layers[0].stateMachine;

        // Single "Idle" state — loops forever, no exit transitions needed.
        var idleState      = rootSM.AddState("Idle");
        idleState.motion   = AssetDatabase.LoadAssetAtPath<AnimationClip>(CLIP_PATH);
        idleState.speed    = 1f;
        rootSM.defaultState = idleState;

        AssetDatabase.SaveAssets();
        Debug.Log("[PlaybuttonSetup] AnimatorController saved: " + CONTROLLER_PATH);

        // ── 6. Attach Animator ────────────────────────────────────────────────
        // ?? does NOT work with Unity objects — GetComponent returns Unity fake-null
        // (not C# null), so ?? never fires. Use == null which Unity overloads correctly.
        Animator animator = pb.GetComponent<Animator>();
        if (animator == null) animator = pb.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.updateMode  = AnimatorUpdateMode.UnscaledTime;  // survives pause
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        Debug.Log("[PlaybuttonSetup] Animator attached — UnscaledTime, AlwaysAnimate.");

        // ── 7. Attach PlaybuttonAnimator (click feedback) ─────────────────────
        if (pb.GetComponent<PlaybuttonAnimator>() == null)
            pb.AddComponent<PlaybuttonAnimator>();
        Debug.Log("[PlaybuttonSetup] PlaybuttonAnimator attached.");

        // ── 8. Guard Button transition ────────────────────────────────────────
        var btn = pb.GetComponent<UnityEngine.UI.Button>();
        if (btn != null && btn.transition == UnityEngine.UI.Selectable.Transition.Animation)
        {
            btn.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            Debug.LogWarning("[PlaybuttonSetup] Button.transition was 'Animation' — reset to ColorTint " +
                             "to prevent UI state-machine from hijacking the Animator.");
        }
        Debug.Log($"[PlaybuttonSetup] Button.transition={btn?.transition} — click events preserved.");

        // ── 9. Save scene ─────────────────────────────────────────────────────
        EditorUtility.SetDirty(pb);
        EditorSceneManager.MarkSceneDirty(pb.scene);
        EditorSceneManager.SaveScene(pb.scene);
        Debug.Log("[PlaybuttonSetup] ✓ Complete — Press Play to preview animations.");
    }

    // Smooth cosine-shaped curve through three values (v0 → vPeak → v0).
    // TangentMode.Auto gives true ease-in/out at all three keyframes.
    static AnimationCurve BuildSineCurve(float v0, float vPeak, float duration)
    {
        var curve = new AnimationCurve(
            new Keyframe(0f,              v0,    0f, 0f),
            new Keyframe(duration * 0.5f, vPeak, 0f, 0f),
            new Keyframe(duration,        v0,    0f, 0f));

        for (int i = 0; i < curve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode( curve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
        }
        return curve;
    }
}
