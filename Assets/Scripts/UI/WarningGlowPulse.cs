using UnityEngine;
using UnityEngine.UI;

// Renders a pulsing orange-red aura behind EnemyWarning using the same Warning_img sprite.
//
// Placement: In Start() this object moves itself to the sibling index just before
// EnemyWarning so it always renders behind it, regardless of hierarchy build order.
//
// Visibility: Mirrors EnemyWarning.gameObject.activeSelf via Image.enabled so the
// glow appears and disappears in sync with the warning (including its alpha flashes).
[RequireComponent(typeof(Image))]
public class WarningGlowPulse : MonoBehaviour
{
    [SerializeField] private float minFactor  = 1.08f;
    [SerializeField] private float maxFactor  = 1.15f;
    [SerializeField] private float pulseSpeed = 1.5f;  // full cycle ≈ 1.33 s
    [SerializeField] private float glowAlpha  = 0.5f;

    // EnemyWarning.cs always resets localScale to exactly (3, 1.5, 1) before each show.
    private static readonly Vector3 k_Base = new Vector3(3f, 1.5f, 1f);

    private Image _image;
    private Image _sourceImage;
    private bool  _wasVisible;

    void Awake() => _image = GetComponent<Image>();

    void Start()
    {
        // Self-order: place just before EnemyWarning so we render behind it.
        // Called after all Awakes, so EnemyWarning.Instance is guaranteed set.
        if (EnemyWarning.Instance != null)
            transform.SetSiblingIndex(EnemyWarning.Instance.transform.GetSiblingIndex());

        // Start hidden — Update will enable when EnemyWarning activates.
        if (_image != null) _image.enabled = false;
    }

    void Update()
    {
        if (EnemyWarning.Instance == null) return;

        bool shouldShow = EnemyWarning.Instance.gameObject.activeSelf;

        if (_wasVisible != shouldShow)
        {
            _wasVisible = shouldShow;
            _image.enabled = shouldShow;
        }

        if (!shouldShow) return;

        // Lazy-resolve source image (EnemyWarning may not have been active during Start)
        if (_sourceImage == null)
            _sourceImage = EnemyWarning.Instance.GetComponent<Image>();

        // Scale pulse: smooth sine wave between minFactor and maxFactor
        float t      = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI) + 1f) * 0.5f;
        float factor = Mathf.Lerp(minFactor, maxFactor, t);
        transform.localScale = new Vector3(k_Base.x * factor, k_Base.y * factor, 1f);

        // Alpha: mirrors EnemyWarning's flash so glow disappears on off-frames
        if (_sourceImage != null)
        {
            Color c = _image.color;
            c.a = glowAlpha * _sourceImage.color.a;
            _image.color = c;
        }
    }
}
