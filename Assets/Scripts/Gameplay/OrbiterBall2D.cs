using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class OrbiterBall2D : MonoBehaviour
{
    public Color ballColor = new Color(0f, 0.82f, 1f);

    public  bool              HasHit;
    // FIX Bug 1: separate flag for enemy contact so the offscreen check can
    // distinguish "missed everything" from "hit an enemy ship". HasHit is set by
    // both TargetRing2D and EnemyShip2D; HitEnemy is set ONLY by EnemyShip2D.
    // This prevents LoseLife() firing a second time when the ball flies offscreen
    // after destroying an enemy (EnemyShip2D already called LoseLife directly).
    public  bool              HitEnemy;
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

        // FIX: ensure the ball's collider is a trigger so OnTriggerEnter2D fires
        // on EnemyShip2D when the ball passes through it.
        // The collider stays disabled until Release() to avoid premature collisions.
        var ballCol = GetComponent<CircleCollider2D>();
        if (ballCol != null) ballCol.isTrigger = true;
    }

    public bool IsReleased => _released;

    // Detects timeScale transitions to toggle Rigidbody2D.simulated.
    private float _prevTimeScale = 1f;

    public void SetHighlight(bool on)
    {
        IsHighlighted = on;
        _sr.color = on ? new Color(1f, 0.8f, 0f, 1f) : ballColor;
    }

    // ── Release ───────────────────────────────────────────────────────────────────
    public void Release()
    {
        if (_released) return;

        // Guard A — game is paused: never launch while the pause panel is active.
        if (PauseMenuUI.IsPaused)
        {
            Debug.LogWarning("[OrbiterBall] Release() blocked — IsPaused=true.");
            return;
        }

        // Guard B — input drain window: the touch that closed the pause panel is
        // still live in the Input system. InputBlocked stays true for 0.15 s after
        // the panel closes so that resume-touch cannot accidentally launch the ball.
        // Only a NEW, separate tap after InputBlocked=false will reach this point.
        if (PauseMenuUI.InputBlocked)
        {
            Debug.LogWarning("[OrbiterBall] Release() blocked — InputBlocked=true " +
                             "(resume-touch drain window still active).");
            return;
        }

        _released = true;
        SetHighlight(false);

        Vector2 vel    = _orbit.GetTangentVelocity();
        _orbit.enabled = false;

        _rb.bodyType       = RigidbodyType2D.Dynamic;
        _rb.gravityScale   = 0f;
        _rb.linearVelocity = vel;
        _rb.simulated      = true;   // ensure physics is active on release

        // FIX: Continuous mode sweeps the collider along its full path between frames.
        // Discrete mode only checks the position AT each frame, so a fast ball can jump
        // completely through a thin EnemyShip2D collider without triggering a hit.
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        GetComponent<CircleCollider2D>().enabled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[OrbiterBall] Hit: {other.gameObject.name}  tag={other.tag}  released={_released}");
    }

    void Update()
    {
        // ── Rigidbody2D.simulated toggle on pause / resume ────────────────────────
        // simulated=false removes the body from physics entirely (no velocity
        // integration, no collision callbacks) — stronger than timeScale=0 alone.
        // Velocity is preserved internally so the ball resumes its exact trajectory.
        float ts = Time.timeScale;
        if (!Mathf.Approximately(ts, _prevTimeScale))
        {
            if (ts < _prevTimeScale)
            {
                _rb.simulated = false;
                Debug.Log($"[OrbiterBall] PAUSE — simulated=false " +
                          $"| released={_released} vel={_rb.linearVelocity} pos={transform.position}");
            }
            else
            {
                _rb.simulated = true;
                Debug.Log($"[OrbiterBall] RESUME — simulated=true " +
                          $"| released={_released} vel={_rb.linearVelocity} pos={transform.position}");
            }
            _prevTimeScale = ts;
        }

        // ── Off-screen check (released balls only) ────────────────────────────────
        if (!_released) return;

        // Never call LoseLife() while paused. Update() still runs at timeScale=0.
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameManager.GameState.Playing) return;

        Camera cam = Camera.main;
        bool offScreen;
        if (cam != null)
        {
            Vector3 vp = cam.WorldToViewportPoint(transform.position);
            offScreen = vp.x < -0.05f || vp.x > 1.05f || vp.y < -0.05f || vp.y > 1.05f;
        }
        else
        {
            offScreen = transform.position.magnitude > 8f;
        }

        if (offScreen)
        {
            // FIX Bug 1: do NOT call LoseLife() if the ball hit an enemy ship.
            // EnemyShip2D.OnTriggerEnter2D() already called LoseLife() and set
            // HitEnemy=true. Without this guard the player loses an extra life simply
            // because the ball continued offscreen after destroying the enemy.
            // HasHit guards against double-scoring on TargetRing hits (existing).
            // HitEnemy guards against double life-loss on enemy hits (new).
            if (!HasHit && !HitEnemy)
                GameManager.Instance?.LoseLife();
            Destroy(gameObject);
        }
    }
}
