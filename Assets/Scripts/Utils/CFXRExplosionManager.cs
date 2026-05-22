using UnityEngine;

// Attach to: "ExplosionManager" GameObject in the Game scene.
// Wire the HitBurst3D prefab in the Inspector.
// Called via: ParticleManager.SpawnHitBurst -> SpawnExplosion(transform.position)
public class CFXRExplosionManager : MonoBehaviour
{
    public static CFXRExplosionManager Instance { get; private set; }

    [Header("Explosion")]
    [Tooltip("Drag the HitBurst3D prefab here.")]
    [SerializeField] private GameObject explosionPrefab;

    [Header("Rendering")]
    [Tooltip("100 ensures the burst renders above all UI (HUD Canvas = 50).")]
    [SerializeField] private int    sortingOrder = 100;
    [SerializeField] private string sortingLayer = "Default";

    [Header("Scale")]
    [SerializeField] private float effectScale = 1.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SpawnExplosion(Vector3 targetPosition)
    {
        if (explosionPrefab == null)
        {
            Debug.LogWarning("[CFXRExplosionManager] explosionPrefab not assigned in Inspector.");
            return;
        }

        GameObject vfx = Instantiate(explosionPrefab, targetPosition, Quaternion.identity);
        vfx.transform.localScale = Vector3.one * effectScale;

        foreach (ParticleSystemRenderer pr in vfx.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            pr.sortingLayerName = sortingLayer;
            pr.sortingOrder     = sortingOrder;
        }

        Debug.Log("Explosion spawned at: " + targetPosition);

        Destroy(vfx, 2f);
    }
}
