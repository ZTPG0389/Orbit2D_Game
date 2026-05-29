#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AddPopupAnimationToAllCards
{
    static readonly string[] CardPaths =
    {
        "PausePanel/Card",
        "GameOverPanel/Card",
        "WinPanel/Card",
    };

    [MenuItem("Tools/UI/Add Popup Animation To All Cards")]
    public static void AddToAllCards()
    {
        int added   = 0;
        int skipped = 0;

        foreach (string path in CardPaths)
        {
            var card = FindInScene(path);
            if (card == null)
            {
                Debug.LogWarning("[AddPopupAnimation] Not found in scene: " + path);
                skipped++;
                continue;
            }

            // CanvasGroup is required by CardPopupAnimation
            if (card.GetComponent<CanvasGroup>() == null)
                Undo.AddComponent<CanvasGroup>(card);

            var anim = card.GetComponent<CardPopupAnimation>();
            if (anim == null)
            {
                anim = Undo.AddComponent<CardPopupAnimation>(card);
                added++;
                Debug.Log("[AddPopupAnimation] Added to: " + path);
            }
            else
            {
                skipped++;
                Debug.Log("[AddPopupAnimation] Already present on: " + path);
            }

            Undo.RecordObject(anim, "Set playOnEnable");
            anim.playOnEnable = true;
        }

        EditorUtility.SetDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().GetRootGameObjects()[0]);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"Done! Popup animation added to all cards. Added={added} AlreadyPresent={skipped}");
        EditorUtility.DisplayDialog("Done",
            $"Popup animation added to all cards.\nAdded: {added}  Already present: {skipped}", "OK");
    }

    // Finds a GameObject by slash-separated path, searching inactive objects too.
    private static GameObject FindInScene(string path)
    {
        var go = GameObject.Find(path);
        if (go != null) return go;

        string[] parts = path.Split('/');
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name != parts[0]) continue;
            if (parts.Length == 1) return root;

            string remainder = string.Join("/", parts, 1, parts.Length - 1);
            var child = root.transform.Find(remainder);
            if (child != null) return child.gameObject;
        }
        return null;
    }
}
#endif
