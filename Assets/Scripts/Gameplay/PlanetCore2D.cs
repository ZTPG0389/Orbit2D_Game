using UnityEngine;
using System.Collections;

public class PlanetCore2D : MonoBehaviour
{
    private float _baseSize;

    void Start()
    {
        _baseSize = Mathf.Min(ScreenBounds.Width, ScreenBounds.Height) * 0.35f;
        transform.localScale = new Vector3(_baseSize, _baseSize, 1f);
        StartCoroutine(PulsePlanet());
    }

    IEnumerator PulsePlanet()
    {
        while (true)
        {
            float t = 0f;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                float s = _baseSize * (1f + Mathf.Sin(t * Mathf.PI) * 0.03f);
                transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }
    }
}
