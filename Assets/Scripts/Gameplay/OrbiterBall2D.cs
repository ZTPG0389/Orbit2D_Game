using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class OrbiterBall2D : MonoBehaviour
{
    public Color ballColor = new Color(0f, 0.82f, 1f);

    public  bool              HasHit;
    public  bool              IsHighlighted;
    private bool              _released;
    private Rigidbody2D       _rb;
    private OrbitController2D _orbit;
    private TrailRenderer     _trail;
    private SpriteRenderer    _sr;

    void Awake()
    {
        _rb    = GetComponent<Rigidbody2D>();
        _orbit = GetComponent<OrbitController2D>();
        _trail = GetComponent<TrailRenderer>();
        _sr    = GetComponent<SpriteRenderer>();
        _sr.color = ballColor;
    }

    public bool IsReleased => _released;

    public void SetHighlight(bool on)
    {
        IsHighlighted = on;
        _sr.color = on ? new Color(1f, 0.8f, 0f, 1f) : ballColor;
    }

    public void Release()
    {
        if (_released) return;
        _released = true;
        SetHighlight(false);

        Vector2 vel    = _orbit.GetTangentVelocity();
        _orbit.enabled = false;

        _rb.bodyType       = RigidbodyType2D.Dynamic;
        _rb.gravityScale   = 0f;
        _rb.linearVelocity = vel;

        GetComponent<CircleCollider2D>().enabled = true;
    }

    void Update()
    {
        if (_released && transform.position.magnitude > 20f)
        {
            if (!HasHit)
                GameManager.Instance?.LoseLife();
            Destroy(gameObject);
        }
    }
}
