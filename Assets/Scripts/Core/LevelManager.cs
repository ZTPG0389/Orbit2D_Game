using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [System.Serializable]
    public struct LevelConfig
    {
        public int targetCount;
        public int orbiterCount;
        public float[] orbiterSpeeds;
        public float[] orbiterRadii;
    }

    [SerializeField] private LevelConfig[] levels;
    [SerializeField] private GameObject orbiterPrefab;
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Transform spawnRoot;

    private readonly List<GameObject> _orbiterObjects = new List<GameObject>();
    private readonly List<GameObject> _targetObjects = new List<GameObject>();

    public int TargetsRemaining { get; private set; }
    public event Action<int> OnTargetsChanged;

    private LevelConfig _currentConfig;
    private int _currentLevel;
    private List<float> _savedRadii = new List<float>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        Debug.Log($"[LevelManager] State -> {state}");
        switch (state)
        {
            case GameManager.GameState.GameOver:
            case GameManager.GameState.MainMenu:
                ClearAll();
                break;
        }
    }

    private int GetTargetCount(int level)
    {
        switch (level)
        {
            case 1:  return 2;
            case 2:  return 3;
            case 3:  return 3;
            case 4:  return 4;
            case 5:  return 4;
            case 6:  return 5;
            case 7:  return 5;
            case 8:  return 6;
            case 9:  return 6;
            case 10: return 7;
            case 11: return 7;
            case 12: return 8;
            case 13: return 9;
            case 14: return 10;
            case 15: return 12;
            default: return 12;
        }
    }

    private int GetOrbiterCount(int level)
    {
        switch (level)
        {
            case 1:
            case 2: return 3;
            default: return 2;
        }
    }

    private float GetOrbiterSpeed(int level)
    {
        switch (level)
        {
            case 1:  return 60f;
            case 2:  return 70f;
            case 3:  return 80f;
            case 4:  return 90f;
            case 5:  return 95f;
            case 6:  return 100f;
            case 7:  return 105f;
            case 8:  return 110f;
            case 9:  return 115f;
            case 10: return 120f;
            case 11: return 130f;
            case 12: return 140f;
            case 13: return 150f;
            case 14: return 158f;
            case 15: return 165f;
            default: return 165f;
        }
    }

    public void LoadLevel(int levelNumber)
    {
        Debug.Log($"[LevelManager] LoadLevel({levelNumber})");
        ClearAll();
        _currentLevel = levelNumber;
        _currentConfig = new LevelConfig
        {
            targetCount = GetTargetCount(levelNumber),
            orbiterCount = GetOrbiterCount(levelNumber),
            orbiterSpeeds = new float[0],
            orbiterRadii = new float[0]
        };
        SpawnTargets(_currentConfig);
        SpawnOrbiters(_currentConfig);
        TargetsRemaining = _currentConfig.targetCount;
        OnTargetsChanged?.Invoke(TargetsRemaining);
    }

    public void RespawnOrbiters()
    {
        foreach (var obj in _orbiterObjects)
            if (obj != null) Destroy(obj);
        _orbiterObjects.Clear();

        int count = TargetsRemaining;
        if (count <= 0) return;

        BallLauncher2D launcher = FindObjectOfType<BallLauncher2D>();
        launcher?.ClearBalls();

        float angleStep = 360f / Mathf.Max(count, 1);
        float baseSpeed = GetOrbiterSpeed(_currentLevel);

        for (int i = 0; i < count; i++)
        {
            float r = 1.2f;

            GameObject go = Instantiate(orbiterPrefab);
            _orbiterObjects.Add(go);

            var oc = go.GetComponent<OrbitController2D>();
            if (oc != null)
            {
                oc.radius = r;
                oc._angle = angleStep * i;
                oc.angularSpeed = baseSpeed * (i % 2 == 0 ? 1f : -1f);

                float rad = oc._angle * Mathf.Deg2Rad;
                go.transform.position = new Vector3(
                    Mathf.Cos(rad) * r,
                    Mathf.Sin(rad) * r,
                    0f
                );
            }

            launcher?.RegisterBall(go.GetComponent<OrbiterBall2D>());
        }
    }

    public void OnTargetHit()
    {
        TargetsRemaining--;
        OnTargetsChanged?.Invoke(TargetsRemaining);
        if (TargetsRemaining <= 0)
            GameManager.Instance?.OnLevelComplete();
    }

    private void SpawnOrbiters(LevelConfig cfg)
    {
        if (orbiterPrefab == null) { Debug.LogError("[LevelManager] orbiterPrefab is NULL"); return; }

        BallLauncher2D launcher = FindObjectOfType<BallLauncher2D>();
        launcher?.ResetForNewLevel();

        _savedRadii.Clear();
        int count = GetOrbiterCount(_currentLevel);
        float angleStep = 360f / Mathf.Max(count, 1);
        float baseSpeed = GetOrbiterSpeed(_currentLevel);

        for (int i = 0; i < count; i++)
        {
            float r = 1.2f;
            _savedRadii.Add(r);

            GameObject go = Instantiate(orbiterPrefab);
            _orbiterObjects.Add(go);

            var oc = go.GetComponent<OrbitController2D>();
            if (oc != null)
            {
                oc.radius = r;
                oc._angle = angleStep * i;
                oc.angularSpeed = baseSpeed * (i % 2 == 0 ? 1f : -1f);
            }

            launcher?.RegisterBall(go.GetComponent<OrbiterBall2D>());
        }
    }

    private void SpawnTargets(LevelConfig cfg)
    {
        if (targetPrefab == null) { Debug.LogError("[LevelManager] targetPrefab is NULL"); return; }

        Transform root = spawnRoot != null ? spawnRoot : transform;

        for (int i = 0; i < cfg.targetCount; i++)
        {
            Vector3 pos = ScreenBounds.RandomTargetPosition(3.0f, 0.75f);
            if (spawnRoot != null) pos += spawnRoot.position;
            GameObject go = Instantiate(targetPrefab, pos, Quaternion.identity, root);
            _targetObjects.Add(go);
        }
    }

    private void ClearOrbiters()
    {
        foreach (GameObject go in _orbiterObjects) if (go != null) Destroy(go);
        _orbiterObjects.Clear();
    }

    private void ClearAll()
    {
        ClearOrbiters();
        foreach (GameObject go in _targetObjects) if (go != null) Destroy(go);
        _targetObjects.Clear();
        TargetsRemaining = 0;
        OnTargetsChanged?.Invoke(0);
    }
}
