using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    [SerializeField] Canvas targetCanvas;

    void Awake()
    {
        Instance = this;
        if (targetCanvas == null)
            targetCanvas = FindFirstObjectByType<Canvas>();
    }

    public static void Show(string text, Vector3 worldPos, Color color)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.Animate(text, worldPos, color));
    }

    IEnumerator Animate(string text, Vector3 worldPos, Color color)
    {
        Canvas canvas = targetCanvas;
        if (canvas == null) yield break;

        var go = new GameObject("FloatingText");
        go.SetActive(false);                            // hide while building to suppress default TMP text flash
        go.transform.SetParent(canvas.transform, false);

        var le = go.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.color         = color;
        tmp.fontSize      = 22f;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 40f);

        // World → screen → canvas local position
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector2 screenPt = cam.WorldToScreenPoint(worldPos);
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), screenPt, uiCam, out Vector2 localPt);
            rt.localPosition = localPt;
        }

        go.SetActive(true);                             // all properties set — safe to show

        Vector3 startLocal = rt.localPosition;
        float elapsed = 0f, duration = 1.2f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            rt.localPosition = startLocal + new Vector3(0f, t * 80f, 0f);
            tmp.color = new Color(color.r, color.g, color.b, 1f - t);
            yield return null;
        }

        Destroy(go);
    }
}
