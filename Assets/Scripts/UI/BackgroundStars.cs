using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to an empty RectTransform that covers the full screen.
/// Spawns random white dots and twinkles them via alpha animation.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class BackgroundStars : MonoBehaviour
{
    [SerializeField] private int   starCount    = 120;
    [SerializeField] private float minSize      = 1.5f;
    [SerializeField] private float maxSize      = 5f;
    [SerializeField] private float twinkleSpeed = 1.2f;

    private Image[] _imgs;
    private float[] _phaseOffsets;

    private void Awake()
    {
        SpawnStars();
    }

    private void Update()
    {
        float t = Time.time * twinkleSpeed;
        for (int i = 0; i < _imgs.Length; i++)
        {
            if (_imgs[i] == null) continue;
            float a = Mathf.Lerp(0.12f, 0.80f,
                (Mathf.Sin(t + _phaseOffsets[i]) + 1f) * 0.5f);
            var c = _imgs[i].color;
            c.a = a;
            _imgs[i].color = c;
        }
    }

    private void SpawnStars()
    {
        _imgs         = new Image[starCount];
        _phaseOffsets = new float[starCount];

        for (int i = 0; i < starCount; i++)
        {
            var go = new GameObject($"Star_{i}");
            go.transform.SetParent(transform, false);

            var rt = go.AddComponent<RectTransform>();
            float s = Random.Range(minSize, maxSize);
            rt.sizeDelta          = new Vector2(s, s);
            rt.anchorMin          = rt.anchorMax = new Vector2(Random.value, Random.value);
            rt.anchoredPosition   = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color         = new Color(1f, 1f, 1f, Random.Range(0.2f, 0.7f));
            img.raycastTarget = false;

            _imgs[i]         = img;
            _phaseOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
        }
    }
}
