using UnityEngine;

// Step 40 — Screen.safeArea -> anchorMin/anchorMax on Canvas RectTransform (notch, Dynamic Island, punch-hole)
public class SafeAreaHandler : MonoBehaviour
{
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        Apply();
    }

    private void Apply()
    {
        if (_rect == null) return;

        Rect    safe = Screen.safeArea;
        Vector2 min  = safe.position;
        Vector2 max  = safe.position + safe.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        _rect.anchorMin = min;
        _rect.anchorMax = max;
    }
}
