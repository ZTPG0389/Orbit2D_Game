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
            Debug.Log($"[EnemySpawner] level={level} < 10 — spawning suppressed.");
            return;
        }
        _spawnLoop = StartCoroutine(SpawnLoop());
        Debug.Log($"[EnemySpawner] SpawnLoop coroutine started for level={level}.");
    }

    public void StopSpawning()
    {
        if (this == null || !gameObject) return;
        StopAllCoroutines();
        _spawnLoop = null;
    }

    IEnumerator SpawnLoop()
    {
        Debug.Log($"[EnemySpawner] SpawnLoop ENTERED — level={_currentLevel}");
        while (true)
        {
            float interval = _currentLevel >= 31 ? 5f
                           : _currentLevel >= 21 ? 6f : 8f;
            Debug.Log($"[EnemySpawner] SpawnLoop waiting {interval}s — live enemies={_enemies.Count}");
            yield return new WaitForSeconds(interval);

            _enemies.RemoveAll(e => e == null);
            Debug.Log($"[EnemySpawner] SpawnLoop tick — enemies after cleanup={_enemies.Count}");

            if (_enemies.Count >= 3)
            {
                Debug.Log("[EnemySpawner] Enemy cap (3) reached — skipping spawn this tick.");
                continue;
            }

            int count = _currentLevel >= 31 ? 2 : 1;
            Debug.Log($"[EnemySpawner] Spawning {count} enemy/enemies.");
            for (int i = 0; i < count; i++)
                SpawnEnemy();
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
        var spr = Resources.Load<Sprite>("Sprites/UI/enemy_ship_red");
        Debug.Log($"[EnemySpawner] Resources.Load sprite 'Sprites/UI/enemy_ship_red' — {(spr != null ? "OK" : "NULL — ship will be invisible!")}");
        if (spr != null) sr.sprite = spr;

        go.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        var col       = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.4f;

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

        Debug.Log($"[EnemySpawner] Enemy created — name='{go.name}' pos={go.transform.position} active={go.activeInHierarchy} totalTracked={_enemies.Count}");
    }

    public void ShowRedFlash() => ScreenFlash.Instance?.FlashRed();
}
