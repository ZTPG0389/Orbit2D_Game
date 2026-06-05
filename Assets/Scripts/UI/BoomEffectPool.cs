using System.Collections.Generic;
using UnityEngine;

public class BoomEffectPool : MonoBehaviour
{
    public static BoomEffectPool Instance;

    [SerializeField] BoomText prefab;
    [SerializeField] int initialSize = 4;

    readonly Queue<BoomText> _pool = new Queue<BoomText>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        for (int i = 0; i < initialSize; i++)
            _pool.Enqueue(CreateInstance());
    }

    public void ShowBoom(Vector3 worldPos)
    {
        BoomText item = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();
        item.Play(worldPos);
    }

    public void Return(BoomText item)
    {
        _pool.Enqueue(item);
    }

    BoomText CreateInstance()
    {
        BoomText b = Instantiate(prefab, transform);
        b.gameObject.SetActive(false);
        return b;
    }
}
