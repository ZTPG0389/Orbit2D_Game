using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class TargetRing2D : MonoBehaviour
{
    private bool _hit;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_hit || !other.CompareTag("Ball")) return;
        _hit = true;

        OrbiterBall2D ball = other.GetComponent<OrbiterBall2D>();
        if (ball != null) ball.HasHit = true;

        ScoreManager.Instance?.RegisterHit();
        LevelManager.Instance?.OnTargetHit();
        ParticleManager.Instance?.SpawnHitBurst(transform.position);
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.Hit);

        Destroy(gameObject);
    }
}
