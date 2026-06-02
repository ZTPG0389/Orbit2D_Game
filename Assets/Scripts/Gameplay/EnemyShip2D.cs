using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyShip2D : MonoBehaviour
{
    public float speed = 2f;
    private Vector3 _targetPos;
    private bool _hit;

    [SerializeField] GameObject explosionPrefab;

    void Awake()
    {
        if (explosionPrefab == null)
            explosionPrefab = Resources.Load<GameObject>("Effects/CFXR2 WW Explosion");

        var col = GetComponent<CircleCollider2D>();
        Debug.Log($"[Enemy] '{gameObject.name}' Awake | explosionPrefab={(explosionPrefab != null ? "LOADED" : "NULL")} " +
                  $"col.isTrigger={col?.isTrigger} col.enabled={col?.enabled} col.radius={col?.radius}");
    }

    public void Init(Vector3 startPos, Vector3 endPos, float spd)
    {
        transform.position = startPos;
        _targetPos         = endPos;
        speed              = spd;

        var spr = Resources.Load<Sprite>("Sprites/UI/enemy_ship_red");
        if (spr != null) GetComponent<SpriteRenderer>().sprite = spr;

        transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        Vector3 dir = (endPos - startPos).normalized;
        if (dir != Vector3.zero)
            transform.up = dir;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, _targetPos, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, _targetPos) < 0.1f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        OrbiterBall2D ball = other.GetComponent<OrbiterBall2D>();

        Debug.Log($"[Enemy] '{gameObject.name}' OnTriggerEnter2D | _hit={_hit} " +
                  $"other='{other.gameObject.name}' ball_found={ball != null} " +
                  $"IsReleased={ball?.IsReleased} HasHit={ball?.HasHit}");

        if (_hit) return;
        if (ball == null || !ball.IsReleased) return;

        _hit = true;
        ball.HasHit = true;

        Debug.Log($"[Enemy] '{gameObject.name}' HIT CONFIRMED — destroying enemy");

        SpawnExplosion();
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.Hit);
        EnemySpawner.Instance?.ShowRedFlash();
        Destroy(gameObject);
    }

    void SpawnExplosion()
    {
        Vector3 pos = transform.position;

        if (explosionPrefab != null)
        {
            Destroy(Instantiate(explosionPrefab, pos, Quaternion.identity), 2f);
            return;
        }

        var loaded = Resources.Load<GameObject>("Effects/CFXR2 WW Explosion");
        if (loaded != null)
        {
            Destroy(Instantiate(loaded, pos, Quaternion.identity), 2f);
            return;
        }

        Debug.LogWarning("[Enemy] explosionPrefab NULL and Resources.Load failed — falling back to ParticleManager.SpawnHitBurst.");
        ParticleManager.Instance?.SpawnHitBurst(pos);
    }
}
