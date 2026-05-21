using UnityEngine;

public class PlanetCore2D : MonoBehaviour
{
    [SerializeField] float swingAngle = 15f;  // kitna tilt hoga
    [SerializeField] float swingSpeed = 0.8f; // kitni tezi se swing hoga

    private float _time = 0f;

    void Update()
    {
        _time += Time.deltaTime * swingSpeed;
        // Swing left and right using sin wave
        float angle = Mathf.Sin(_time) * swingAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
