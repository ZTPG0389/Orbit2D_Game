using UnityEngine;
using UnityEngine.UI;

// Attach to the EnemyWarning GameObject (same GO as the Image and EnemyWarning script).
// Adds a red pulsing Outline border that appears and disappears automatically with the GO.
// Outline alpha mirrors EnemyWarning's own Image alpha so the border also flashes in sync.
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public class WarningBorderGlow : MonoBehaviour
{
    [Header("Border Color")]
    [SerializeField] private Color borderColor = new Color(1f, 0f, 0f, 1f);  // pure red

    [Header("Outline Size — increase for thicker glow")]
    [SerializeField] private Vector2 effectDistance = new Vector2(5f, -5f);

    [Header("Pulse")]
    [SerializeField] private float minAlpha   = 0.30f;  // dimmest point
    [SerializeField] private float maxAlpha   = 1.00f;  // brightest point
    [SerializeField] private float pulseSpeed = 2.5f;   // full cycle ≈ 0.4 s (urgent feel)

    private Outline _outline;
    private Image   _img;

    void Awake()
    {
        _img     = GetComponent<Image>();
        _outline = GetComponent<Outline>();

        _outline.effectColor     = new Color(borderColor.r, borderColor.g, borderColor.b, 0f);
        _outline.effectDistance  = effectDistance;
        _outline.useGraphicAlpha = false;
    }

    void OnEnable()
    {
        // Fresh state each time EnemyWarning is shown
        if (_outline != null)
            _outline.effectColor = new Color(borderColor.r, borderColor.g, borderColor.b, 0f);
    }

    void Update()
    {
        // EnemyWarning.cs flashes _img.color.a between 1 (on) and 0 (off).
        // Multiplying keeps the border hidden on off-frames.
        float srcAlpha = _img != null ? _img.color.a : 1f;

        float t     = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t) * srcAlpha;

        Color c = _outline.effectColor;
        c.a = alpha;
        _outline.effectColor = c;
    }
}
