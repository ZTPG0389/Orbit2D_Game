using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    private readonly List<GameObject> _enemies = new List<GameObject>();
    private int      _currentLevel;
    private Coroutine _spawnLoop;

    void Awake()
    {
        Debug.Log($"[EnemySpawner] Awake — existingInstance={(Instance != null ? $"EXISTS active={Instance.gameObject.activeInHierarchy}" : "NULL")} thisActive={gameObject.activeInHierarchy}");
        if (Instance != null && Instance != this)
        {
            Debug.Log("[EnemySpawner] Duplicate detected — destroying self, keeping existing instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[EnemySpawner] Instance set — DontDestroyOnLoad applied.");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartSpawning(int level)
    {
        Debug.Log($"[EnemySpawner] StartSpawning called — level={level} " +
                  $"gameObject.active={gameObject.activeInHierarchy} enabled={enabled} Instance={(Instance != null ? "SET" : "NULL")}");

        // Component may be saved as disabled in the scene — enable it so StartCoroutine works.
        if (!enabled)
        {
            Debug.LogWarning("[EnemySpawner] Component was disabled — enabling now. Check the scene: EnemySpawner MonoBehaviour should be ticked.");
            enabled = true;
        }

        _currentLevel = level;
        StopSpawning();

        if (level < 10)
        {
            // Levels 1–9: spawn exactly one enemy immediately, no repeat loop.
            Debug.Log($"[EnemySpawner] level={level} < 10 — spawning single enemy once.");
            try   { SpawnEnemy(); }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EnemySpawner] SpawnEnemy (single-shot) threw: {ex.GetType().Name}: {ex.Message}");
            }
            return;
        }

        // Level 10+: run the full timed spawn loop as before.
        _spawnLoop = StartCoroutine(SpawnLoop());
        Debug.Log($"[EnemySpawner] SpawnLoop coroutine started for level={level}.");
    }

    public void StopSpawning()
    {
        if (this == null || !gameObject) return;
        StopAllCoroutines();
        _spawnLoop = null;

        // FIX Bug 2: destroy all tracked enemy GameObjects so they do not survive
        // into the next game session. Log confirmed: after RestartCurrentLevel() the
        // new SpawnLoop started with "live enemies=1" — a ghost from the previous run
        // that StopSpawning had left alive. Without this, the cap logic can block
        // fresh spawns and stale enemies keep moving / can still deal damage.
        foreach (var e in _enemies)
            if (e != null) Destroy(e);
        _enemies.Clear();
        Debug.Log("[EnemySpawner] StopSpawning — all enemies destroyed and list cleared.");
    }

    IEnumerator SpawnLoop()
    {
        Debug.Log($"[EnemySpawner] SpawnLoop ENTERED — level={_currentLevel}");
        int tick = 0;
        while (true)
        {
            float interval = _currentLevel >= 31 ? 5f
                           : _currentLevel >= 21 ? 6f : 8f;
            tick++;
            Debug.Log($"[EnemySpawner] SpawnLoop tick #{tick} — waiting {interval}s — live enemies={_enemies.Count}");
            yield return new WaitForSeconds(interval);

            _enemies.RemoveAll(e => e == null);
            Debug.Log($"[EnemySpawner] SpawnLoop tick #{tick} resumed — enemies after cleanup={_enemies.Count}");

            if (_enemies.Count >= 3)
            {
                Debug.Log("[EnemySpawner] Enemy cap (3) reached — skipping spawn this tick.");
                continue;
            }

            int count = _currentLevel >= 31 ? 2 : 1;
            Debug.Log($"[EnemySpawner] Spawning {count} enemy/enemies.");
            for (int i = 0; i < count; i++)
            {
                try
                {
                    SpawnEnemy();
                }
                catch (System.Exception ex)
                {
                    // Without this catch the coroutine dies silently on Android IL2CPP.
                    Debug.LogError($"[EnemySpawner] SpawnEnemy THREW — coroutine survived: " +
                                   $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }

    void SpawnEnemy()
    {
        // --- FIX 1: Camera.main fallback ---
        Camera cam = Camera.main;
        if (cam == null)
        {
            var allCams = FindObjectsOfType<Camera>();
            Debug.LogWarning($"[EnemySpawner] Camera.main is NULL — found {allCams.Length} camera(s) total:");
            foreach (var c in allCams)
                Debug.LogWarning($"  Camera: '{c.name}'  tag='{c.tag}'  active={c.gameObject.activeInHierarchy}  enabled={c.enabled}");
            if (allCams.Length > 0)
            {
                cam = allCams[0];
                Debug.LogWarning($"[EnemySpawner] Falling back to first available camera: '{cam.name}'. Fix: tag it 'MainCamera'.");
            }
            else
            {
                Debug.LogError("[EnemySpawner] No camera found at all — cannot spawn enemy.");
                return;
            }
        }

        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        Debug.Log($"[EnemySpawner] Camera OK — name='{cam.name}' orthoSize={h:F2} aspect={cam.aspect:F3} w={w:F2}");

        int edge = Random.Range(0, 4);
        Vector3 start, end;
        switch (edge)
        {
            case 0:  start = new Vector3(Random.Range(-w, w),  h + 1f, 0f);
                     end   = new Vector3(Random.Range(-w, w), -h - 1f, 0f); break;
            case 1:  start = new Vector3(Random.Range(-w, w), -h - 1f, 0f);
                     end   = new Vector3(Random.Range(-w, w),  h + 1f, 0f); break;
            case 2:  start = new Vector3(-w - 1f, Random.Range(-h, h), 0f);
                     end   = new Vector3( w + 1f, Random.Range(-h, h), 0f); break;
            default: start = new Vector3( w + 1f, Random.Range(-h, h), 0f);
                     end   = new Vector3(-w - 1f, Random.Range(-h, h), 0f); break;
        }
        float spd = Mathf.Clamp(1.5f + (_currentLevel - 10) * 0.1f, 1.5f, 4f);
        Debug.Log($"[EnemySpawner] Spawn — edge={edge} start={start} end={end} spd={spd:F2}");

        var go = new GameObject("EnemyShip");

        var sr  = go.AddComponent<SpriteRenderer>();
        // Explicit sort order — without this the enemy renders behind background sprites
        // (order 0 default loses to any background with order >= 1).
        sr.sortingLayerName = "Default";
        sr.sortingOrder     = 10;
        sr.color            = Color.white;
        Debug.Log($"[EnemySpawner] SR created — material='{sr.material?.name}' " +
                  $"shader='{sr.material?.shader?.name}' sortOrder={sr.sortingOrder}");

        var spr = Resources.Load<Sprite>("Sprites/UI/enemy_ship_red");
        Debug.Log($"[EnemySpawner] Resources.Load sprite 'Sprites/UI/enemy_ship_red' — {(spr != null ? "OK" : "NULL — ship will be invisible!")}");
        if (spr != null) sr.sprite = spr;

        go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        var col       = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.6f;   // FIX: wider than 0.4 — matches EnemyShip2D.Awake() enforcement

        var trail = go.AddComponent<TrailRenderer>();
        trail.startColor        = new Color(1f, 0.3f, 0f, 1f);
        trail.endColor          = new Color(1f, 0f,   0f, 0f);
        trail.time              = 0.5f;
        trail.startWidth        = 0.15f;
        trail.endWidth          = 0f;
        trail.minVertexDistance = 0.05f;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows    = false;

        // --- FIX 2: Shader.Find("Sprites/Default") returns null in URP Android builds ---
        // Use a Resources-loaded material or fall back to a guaranteed URP shader.
        Material trailMat = Resources.Load<Material>("TrailDefaultMat");
        if (trailMat == null)
        {
            Shader trailShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                              ?? Shader.Find("Sprites/Default");
            Debug.Log($"[EnemySpawner] Trail shader — '{(trailShader != null ? trailShader.name : "NULL — trail will be broken")}' ");
            if (trailShader != null)
                trailMat = new Material(trailShader);
        }
        if (trailMat != null)
            trail.material = trailMat;

        var enemy = go.AddComponent<EnemyShip2D>();
        enemy.Init(start, end, spd);
        _enemies.Add(go);

        // Final-state dump — every field that can hide an enemy on Android.
        var fsr = go.GetComponent<SpriteRenderer>();
        Debug.Log($"[EnemySpawner] Enemy FINAL STATE — name='{go.name}' " +
                  $"pos={go.transform.position} active={go.activeInHierarchy} " +
                  $"srEnabled={fsr?.enabled} sprite={(fsr?.sprite != null ? fsr.sprite.name : "NULL")} " +
                  $"material='{fsr?.material?.name}' shader='{fsr?.material?.shader?.name}' " +
                  $"color={fsr?.color} sortLayer='{fsr?.sortingLayerName}' sortOrder={fsr?.sortingOrder} " +
                  $"totalTracked={_enemies.Count}");
    }

    public void ShowRedFlash() => ScreenFlash.Instance?.FlashRed();
}
