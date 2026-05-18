using UnityEngine;

public class OrbitController2D : MonoBehaviour
{
    public float radius       = 2.5f;
    public float angularSpeed = 120f;

    public float _angle = 0f;

    public Vector2 GetTangentVelocity()
    {
        float rad = _angle * Mathf.Deg2Rad;
        return new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad)) * angularSpeed * Mathf.Deg2Rad * radius * 2.8f;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing) return;
        _angle += angularSpeed * Time.deltaTime;
        float rad = _angle * Mathf.Deg2Rad;
        transform.localPosition = new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
    }
}
