using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class FixButtonShadow
{
    [MenuItem("OrbitDrop3D/Remove Button Shadow")]
    static void Fix()
    {
        using (var scope = new PrefabUtility.EditPrefabContentsScope("Assets/Prefabs/LvlBtn_01.prefab"))
        {
            Transform root = scope.prefabContentsRoot.transform;

            // The root Image is the shadow: color (0,0,0,0.4) with a background sprite.
            // It is also the Button's TargetGraphic — keep the component enabled so
            // raycasting (tap detection) still works; just make it fully invisible.
            var img = root.GetComponent<Image>();
            if (img == null) { Debug.LogError("[FixShadow] No Image on root."); return; }

            Color oldColor  = img.color;
            Sprite oldSprite = img.sprite;

            img.color  = new Color(0f, 0f, 0f, 0f); // fully transparent — no visible square
            img.sprite = null;                        // no sprite — nothing to draw

            Debug.Log($"[FixShadow] Root Image fixed." +
                      $"\n  color:  {oldColor} -> {img.color}" +
                      $"\n  sprite: {(oldSprite != null ? oldSprite.name : "null")} -> null" +
                      $"\n  Prefab saved. All level buttons will be shadow-free.");
        }
    }
}
