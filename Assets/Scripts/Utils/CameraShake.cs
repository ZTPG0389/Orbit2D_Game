using System.Collections;
using UnityEngine;

// Attach to: the Main Camera in the Game scene.
// Trigger with: CameraShake.Instance.Shake(duration, magnitude)
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 _originalLocalPos;
    private Coroutine _shakeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        _originalLocalPos = transform.localPosition;
    }

    // Shakes the camera for 'duration' seconds. Magnitude is in world units.
    // Shake decays smoothly from full strength to zero.
    public void Shake(float duration = 0.18f, float magnitude = 0.12f)
    {
        if (_shakeRoutine != null)
            StopCoroutine(_shakeRoutine);
        _shakeRoutine = StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Decay: shake is strongest at start, smoothly fades to zero
            float decay = 1f - (elapsed / duration);
            float ox = Random.Range(-1f, 1f) * magnitude * decay;
            float oy = Random.Range(-1f, 1f) * magnitude * decay;
            transform.localPosition = _originalLocalPos + new Vector3(ox, oy, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalLocalPos;
        _shakeRoutine = null;
    }

    // Snap camera back when disabled (pause, game over, scene change).
    private void OnDisable()
    {
        if (_shakeRoutine != null) { StopCoroutine(_shakeRoutine); _shakeRoutine = null; }
        transform.localPosition = _originalLocalPos;
    }
}
