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

        int points = ScoreManager.Instance?.RegisterHit() ?? 0;
        LevelManager.Instance?.OnTargetHit();
        ParticleManager.Instance?.SpawnHitBurst(transform.position);
        FloatingScoreUI.Instance?.ShowScore(transform.position, points);
        AudioManager.Instance?.PlaySFX(AudioManager.SFX.Hit);
        if (PlayerPrefs.GetInt("VibrationOn", 1) == 1)
        {
#if UNITY_ANDROID
            Handheld.Vibrate();
#endif
        }

        Destroy(gameObject);
    }
}
