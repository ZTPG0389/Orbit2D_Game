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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void StartSpawning(int level)
    {
        _currentLevel = level;
        StopSpawning();
        if (level < 10) return;
        _spawnLoop = StartCoroutine(SpawnLoop());
        Debug.Log($"[EnemySpawner] Started — Level={level}");
    }

    public void StopSpawning()
    {
        if (this == null || !gameObject) return;
        StopAllCoroutines();
        _spawnLoop = null;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float interval = _currentLevel >= 31 ? 5f
                           : _currentLevel >= 21 ? 6f : 8f;
            yield return new WaitForSeconds(interval);

            _enemies.RemoveAll(e => e == null);
            if (_enemies.Count >= 3) continue;

            int count = _currentLevel >= 31 ? 2 : 1;
            for (int i = 0; i < count; i++)
                SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;

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

        var go = new GameObject("EnemyShip");

        var sr  = go.AddComponent<SpriteRenderer>();
        var spr = Resources.Load<Sprite>("Sprites/UI/enemy_ship_red");
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
        trail.material          = new Material(Shader.Find("Sprites/Default"));

        var enemy = go.AddComponent<EnemyShip2D>();
        enemy.Init(start, end, spd);
        _enemies.Add(go);
    }

    public void ShowRedFlash() => ScreenFlash.Instance?.FlashRed();
}
