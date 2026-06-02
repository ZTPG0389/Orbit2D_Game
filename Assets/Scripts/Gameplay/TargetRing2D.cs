using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class TargetRing2D : MonoBehaviour
{
    [Header("Wing Animation")]
    [SerializeField] private Transform wingLeft;
    [SerializeField] private Transform wingRight;
    [SerializeField] private float wingFlapSpeed = 1.2f;
    [SerializeField] private float wingFlapAmount = 12f;

    [Header("Body Pulse")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseAmount = 0.04f;

    private Vector3 _visualStartScale;
    private bool _hit;

    void Start()
    {
        var col = GetComponent<CircleCollider2D>();
        Debug.Log($"[TargetRing2D] '{gameObject.name}' (path={GetPath()}) spawned | " +
                  $"tag={gameObject.tag} layer={LayerMask.LayerToName(gameObject.layer)} " +
                  $"col_enabled={col?.enabled} isTrigger={col?.isTrigger} radius={col?.radius}");

        if (visualRoot == null && transform.childCount > 0)
            visualRoot = transform.GetChild(0);

        if (visualRoot != null)
            _visualStartScale = visualRoot.localScale;

        if (wingLeft == null || wingRight == null)
            FindWings();
    }

    string GetPath()
    {
        string path = gameObject.name;
        Transform t = transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }

    void Update()
    {
        if (_hit) return;
        float t = Time.time;

        float wingAngle = Mathf.Sin(t * wingFlapSpeed) * wingFlapAmount;
        if (wingLeft != null)
            wingLeft.localRotation = Quaternion.Euler(0, 0, wingAngle);
        if (wingRight != null)
            wingRight.localRotation = Quaternion.Euler(0, 0, -wingAngle);

        if (visualRoot != null)
        {
            float pulse = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
            visualRoot.localScale = _visualStartScale * pulse;
        }
    }

    void FindWings()
    {
        foreach (Transform child in transform)
        {
            string n = child.name.ToLower();
            if (wingLeft == null &&
                (n.Contains("left") || n.Contains("wing") || n.Contains("solar") || n.Contains("panel")))
            {
                wingLeft = child;
                continue;
            }
            if (wingRight == null && n.Contains("right"))
            {
                wingRight = child;
            }
        }

        if (wingLeft == null && transform.childCount > 0)
            wingLeft = transform.GetChild(0);
        if (wingRight == null && transform.childCount > 1)
            wingRight = transform.GetChild(1);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[TargetRing2D] OnTriggerEnter2D on '{GetPath()}' | _hit={_hit} other='{other.gameObject.name}'");
        if (_hit) return;
        var ball = other.GetComponent<OrbiterBall2D>();
        Debug.Log($"[TargetRing2D] Ball check | ball_found={ball != null} IsReleased={ball?.IsReleased} HasHit={ball?.HasHit}");
        if (ball == null || !ball.IsReleased) return;
        _hit = true;
        Debug.Log($"[TargetRing2D] HIT ACCEPTED on '{GetPath()}' — processing score/destroy");

        StartCoroutine(HitEffect());

        ball.HasHit = true;

        int points = ScoreManager.Instance?.RegisterHit() ?? 0;
        LevelManager.Instance?.OnTargetHit();
        ParticleManager.Instance?.SpawnHitBurst(transform.position);
        FloatingScoreUI.Instance?.ShowScore(transform.position, points);
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.Hit);
        if (PlayerPrefs.GetInt("VibrationOn", 1) == 1)
        {
#if UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        Destroy(gameObject, 0.3f);
    }

    System.Collections.IEnumerator HitEffect()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 originalScale = transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.Rotate(0, 0, 720f * Time.deltaTime);
            float scale = Mathf.Lerp(1.2f, 0f, t);
            transform.localScale = originalScale * scale;
            yield return null;
        }
    }
}
