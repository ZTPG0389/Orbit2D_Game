using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyShip2D : MonoBehaviour
{
    public float speed = 2f;
    private Vector3 _targetPos;

    public void Init(Vector3 startPos, Vector3 endPos, float spd)
    {
        transform.position = startPos;
        _targetPos         = endPos;
        speed              = spd;

        var spr = Resources.Load<Sprite>("Sprites/UI/enemy_ship_red");
        if (spr != null) GetComponent<SpriteRenderer>().sprite = spr;

        // Face direction of travel
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
        if (!other.CompareTag("OrbiterBall")) return;
        GameManager.Instance?.LoseLife();
        EnemySpawner.Instance?.ShowRedFlash();
        FloatingTextManager.Show("-1 LIFE", transform.position, Color.red);
        Destroy(gameObject);
    }
}
