using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private int        prewarmCount = 6;

    private Queue<GameObject> popupPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Prewarm(scorePopupPrefab, popupPool, prewarmCount);
    }

    private void Prewarm(GameObject prefab, Queue<GameObject> pool, int count)
    {
        if (prefab == null) return;
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    // Forwards to CFXRExplosionManager so callers don't need a direct reference.
    public void SpawnHitBurst(Vector3 worldPos)
    {
        CFXRExplosionManager.Instance?.SpawnExplosion(worldPos);
    }

    public void SpawnScorePopup(Vector3 worldPos, int points)
    {
        if (scorePopupPrefab == null || points <= 0) return;

        GameObject go = Rent(popupPool, scorePopupPrefab);
        go.transform.position = worldPos + Vector3.up * 0.4f;
        go.SetActive(true);

        var label = go.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = $"+{points}";

        StartCoroutine(AnimatePopup(go, label));
    }

    private IEnumerator AnimatePopup(GameObject go, TMP_Text label)
    {
        float   duration = 0.85f;
        float   rise     = 1.6f;
        Color   baseCol  = label != null ? label.color : Color.white;
        Vector3 startPos = go.transform.position;
        float   t        = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float c = Mathf.Clamp01(t);
            go.transform.position = startPos + Vector3.up * (rise * c);

            if (label != null)
            {
                float alpha = Mathf.Clamp01(1f - c * c);
                label.color = new Color(baseCol.r, baseCol.g, baseCol.b, alpha);
            }
            yield return null;
        }

        go.SetActive(false);
        popupPool.Enqueue(go);
    }

    private IEnumerator Return(GameObject go, Queue<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go != null)
        {
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    private GameObject Rent(Queue<GameObject> pool, GameObject prefab)
    {
        while (pool.Count > 0)
        {
            var candidate = pool.Dequeue();
            if (candidate != null && !candidate.activeSelf)
                return candidate;
        }
        return Instantiate(prefab, transform);
    }
}
