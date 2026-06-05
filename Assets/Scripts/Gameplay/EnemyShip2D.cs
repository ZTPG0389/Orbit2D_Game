using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyShip2D : MonoBehaviour
{
    public float speed = 2f;
    private Vector3 _targetPos;
    private bool _hit;
    private bool _firstUpdateLogged;

    [SerializeField] GameObject explosionPrefab;

    void Awake()
    {
        if (explosionPrefab == null)
            explosionPrefab = Resources.Load<GameObject>("Effects/CFXR2 WW Explosion");

        var col = GetComponent<CircleCollider2D>();
        // FIX: enforce collider settings here so they apply regardless of how the enemy
        // was instantiated (runtime SpawnEnemy or direct prefab placement).
        // Larger radius reduces tunneling; isTrigger is required for OnTriggerEnter2D.
        if (col != null)
        {
            col.radius    = 0.6f;   // up from 0.4 — wider hit area catches fast OrbiterBall
            col.isTrigger = true;
        }
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
        if (!_firstUpdateLogged)
        {
            _firstUpdateLogged = true;
            var sr = GetComponent<SpriteRenderer>();
            Debug.Log($"[Enemy] '{gameObject.name}' first Update — " +
                      $"pos={transform.position} target={_targetPos} " +
                      $"isVisible={sr?.isVisible} bounds={sr?.bounds} " +
                      $"sprite={(sr?.sprite != null ? sr.sprite.name : "NULL")} " +
                      $"material='{sr?.material?.name}' shader='{sr?.material?.shader?.name}' " +
                      $"color={sr?.color} sortOrder={sr?.sortingOrder} srEnabled={sr?.enabled}");
        }

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

        // CHANGE 1: mark hit immediately so re-entrant callbacks are ignored.
        _hit = true;

        // CHANGE 2: flag the ball so TargetRing2D and off-screen checks know it
        // already registered a hit and should not trigger a second life-loss.
        ball.HasHit = true;

        // Vibrate on impact — gives stronger tactile feedback than the spawn buzz
        // because this event costs the player a life.
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif

        Debug.Log($"[Enemy] '{gameObject.name}' HIT CONFIRMED — destroying enemy");

        // Missile/explosion sound plays on enemy collision — distinct from both
        // SFX.Hit (target-ring score sound) and SFX.Alert (pre-spawn warning).
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.MissileAttack);

        // CHANGE 4: deduct one life — enemy ships are hazards, not targets.
        // No score is awarded here; LoseLife() handles lives-remaining and
        // triggers GameOver when lives reach zero.
        GameManager.Instance?.LoseLife();

        // FIX Bug 1: mark the ball as having hit an enemy BEFORE the enemy is
        // destroyed. OrbiterBall2D.Update() checks HitEnemy in the offscreen guard
        // so it will NOT call LoseLife() a second time when the ball continues
        // offscreen after this collision. Must be set while ball reference is still valid.
        ball.HitEnemy = true;

        // CHANGE 5: spawn explosion VFX at the enemy's position before destroying it.
        SpawnExplosion();

        // Red screen-flash signals the player that damage was taken.
        EnemySpawner.Instance?.ShowRedFlash();

        // CHANGE 6: destroy the enemy ship after all effects are queued.
        Destroy(gameObject);
    }

    void SpawnExplosion()
    {
        Vector3 pos = transform.position;

        BoomEffectPool.Instance?.ShowBoom(pos);

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
