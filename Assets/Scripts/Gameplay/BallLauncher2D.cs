using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallLauncher2D : MonoBehaviour
{
    private List<OrbiterBall2D> _balls          = new List<OrbiterBall2D>();
    private int                 _currentIndex   = 0;
    private bool                _allThrown      = false;
    private bool                _inputCooldown  = false;

    public void ResetForNewLevel()
    {
        StopAllCoroutines();
        CancelInvoke();
        _balls.Clear();
        _currentIndex  = 0;
        _allThrown     = false;
        _inputCooldown = false;
    }

    public void ClearBalls()
    {
        CancelInvoke();
        _balls.Clear();
        _currentIndex  = 0;
        _allThrown     = false;
        _inputCooldown = false;
    }

    public void RegisterBall(OrbiterBall2D ball)
    {
        if (ball == null) return;
        _balls.Add(ball);
        ball.SetHighlight(_balls.Count == 1);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;
        if (_allThrown) return;
        if (_inputCooldown) return;

        bool tapped = Input.GetMouseButtonDown(0);
        if (!tapped && Input.touchCount > 0)
            tapped = Input.GetTouch(0).phase == TouchPhase.Began;

        if (!tapped) return;

        _inputCooldown = true;
        ThrowYellowBall();
        Invoke(nameof(ResetCooldown), 0.3f);
    }

    void ResetCooldown() => _inputCooldown = false;

    void ThrowYellowBall()
    {
        OrbiterBall2D yellowBall = null;
        foreach (var b in _balls)
        {
            if (b != null && !b.IsReleased && b.IsHighlighted)
            {
                yellowBall = b;
                break;
            }
        }

        if (yellowBall == null) return;

        yellowBall.SetHighlight(false);
        yellowBall.Release();

        bool foundNext = false;
        foreach (var b in _balls)
        {
            if (b != null && !b.IsReleased && !b.IsHighlighted)
            {
                b.SetHighlight(true);
                foundNext = true;
                break;
            }
        }

        if (!foundNext)
        {
            _allThrown = true;
            StartCoroutine(CheckAllExited());
        }
    }

    IEnumerator CheckAllExited()
    {
        yield return new WaitForSeconds(1.5f);
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing)
            yield break;
        if (LevelManager.Instance != null && LevelManager.Instance.TargetsRemaining > 0)
        {
            GameManager.Instance.LoseLife();
            if (GameManager.Instance.Lives > 0)
            {
                _allThrown = false;
                LevelManager.Instance.RespawnOrbiters();
            }
        }
    }
}
