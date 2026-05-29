using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class AddBorderOverlay
{
    [MenuItem("Tools/UI/Add Border Glow To GameOver Card")]
    static void Run()
    {
        var panel = GameObject.Find("GameOverPanel");
        if (panel == null)
        {
            Debug.LogError("[AddBorderOverlay] 'GameOverPanel' not found in scene.");
            return;
        }

        var cardTransform = panel.transform.Find("Card");
        if (cardTransform == null)
        {
            Debug.LogError("[AddBorderOverlay] 'Card' not found inside GameOverPanel.");
            return;
        }

        // Remove existing BorderGlow to avoid duplicates
        var existing = cardTransform.Find("BorderGlow");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        // Create BorderGlow child
        var go = new GameObject("BorderGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(cardTransform, false);
        go.transform.SetAsLastSibling();

        // Stretch to fill Card
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = Vector2.zero;

        // Image settings
        var img = go.GetComponent<Image>();
        img.sprite        = Resources.Load<Sprite>("Sprites/UI/card_border_glow");
        img.color         = Color.white;
        img.type          = Image.Type.Simple;
        img.raycastTarget = false;

        if (img.sprite == null)
            Debug.LogWarning("[AddBorderOverlay] Sprite not found at Resources/Sprites/UI/card_border_glow. " +
                             "Assign it manually in the Inspector.");

        EditorUtility.SetDirty(cardTransform.gameObject);
        EditorSceneManager.MarkSceneDirty(cardTransform.gameObject.scene);

        Selection.activeGameObject = go;
        Debug.Log("BorderGlow added!");
    }
}
