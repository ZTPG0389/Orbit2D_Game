using UnityEngine;

// Step 35 — Orthographic size calculation for all aspect ratios (9:16 reference)
public class CameraSetup : MonoBehaviour
{
    [SerializeField] private float referenceHeight = 11f;  // orthographicSize = 5.5f on 9:16

    private const float ReferenceAspect = 9f / 16f; // portrait reference

    private void Awake()
    {
        var cam = GetComponent<Camera>();
        if (cam == null || !cam.orthographic) return;

        float screenAspect = (float)Screen.width / Screen.height;

        // On narrower screens than the reference, expand vertical view
        // so the reference horizontal span is always fully visible.
        if (screenAspect < ReferenceAspect)
            cam.orthographicSize = (referenceHeight / 2f) * (ReferenceAspect / screenAspect);
        else
            cam.orthographicSize = referenceHeight / 2f;
    }
}
