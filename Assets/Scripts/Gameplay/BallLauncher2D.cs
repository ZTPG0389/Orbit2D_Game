using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallLauncher2D : MonoBehaviour
{
    private List<OrbiterBall2D> _balls             = new List<OrbiterBall2D>();
    private bool                _waitingForRespawn = false;

    // Called by LevelManager before registering new balls for a fresh level.
    public void ResetForNewLevel()
    {
        StopAllCoroutines();
        _balls.Clear();
        _waitingForRespawn = false;
    }

    // Called by LevelManager.RespawnOrbiters() before registering new balls.
    public void ClearBalls()
    {
        _balls.Clear();
        _waitingForRespawn = false;
    }

    public void RegisterBall(OrbiterBall2D ball)
    {
        if (ball != null) _balls.Add(ball);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing) return;

        bool tapped = Input.GetMouseButtonDown(0);
        if (!tapped && Input.touchCount > 0)
            tapped = Input.GetTouch(0).phase == TouchPhase.Began;

        if (!tapped) return;

        // Purge destroyed ball refs accumulated from previous levels.
        _balls.RemoveAll(b => b == null);

        Vector2 tapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        OrbiterBall2D closest = null;
        float minDist = float.MaxValue;
        foreach (var b in _balls)
        {
            if (b.IsReleased) continue;
            float d = Vector2.Distance(tapPos, b.transform.position);
            if (d < minDist) { minDist = d; closest = b; }
        }

        if (closest != null)
        {
            closest.Release();
            _balls.Remove(closest);
        }

        // All balls released — start respawn timer (guard prevents duplicate coroutines).
        if (!_waitingForRespawn && !_balls.Exists(b => !b.IsReleased))
        {
            _waitingForRespawn = true;
            StartCoroutine(CheckAllExited());
        }
    }

    IEnumerator CheckAllExited()
    {
        yield return new WaitForSeconds(1.5f);
        _waitingForRespawn = false;

        if (GameManager.Instance == null
            || GameManager.Instance.State != GameManager.GameState.Playing)
            yield break;

        if (LevelManager.Instance != null && LevelManager.Instance.TargetsRemaining > 0)
        {
            GameManager.Instance.LoseLife();
            if (GameManager.Instance.Lives > 0)
                LevelManager.Instance.RespawnOrbiters();
        }
    }
}
