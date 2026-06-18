using UnityEngine;
using UnityEngine.EventSystems;   // required for EventSystem.IsPointerOverGameObject
using System.Collections.Generic;

public class BallLauncher2D : MonoBehaviour
{
    private List<OrbiterBall2D> _balls          = new List<OrbiterBall2D>();
    private bool                _allThrown      = false;
    private bool                _inputCooldown  = false;
    private bool                _waitingForExit = false;

    // Unscaled timestamp before which all launch input is ignored.
    // Set by BlockInputFor() immediately after resume so the touch that
    // activated the Resume button cannot reach ThrowYellowBall().
    // Uses unscaledTime so it works correctly at any timeScale value.
    private float _resumeBlockUntil = 0f;

    public void ResetForNewLevel()
    {
        CancelInvoke();
        _balls.Clear();
        _allThrown      = false;
        _inputCooldown  = false;
        _waitingForExit = false;
    }

    public void ClearBalls()
    {
        CancelInvoke();
        _balls.Clear();
        _allThrown      = false;
        _inputCooldown  = false;
        _waitingForExit = false;
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

        // Suppress all launch input until the drain window set by BlockInputFor() expires.
        // This catches the resume-touch that arrives in the same frame as State→Playing.
        if (Time.unscaledTime < _resumeBlockUntil) return;

        if (_waitingForExit)
        {
            bool allGone = true;
            foreach (var b in _balls)
                if (b != null) { allGone = false; break; }

            if (allGone)
            {
                _waitingForExit = false;
                OnAllBallsExited();
            }
            return;
        }

        if (_allThrown || _inputCooldown) return;

        // FIX: use UI-aware input so any touch/click that begins on a UI element
        // (Pause button, Resume, Settings, Home, etc.) is never forwarded to gameplay.
        if (!IsTapOnGameWorld()) return;

        _inputCooldown = true;
        ThrowYellowBall();
        Invoke(nameof(ResetCooldown), 0.3f);
    }

    // Returns true only when a tap/click began on the game world — not on any UI element.
    //
    // Root cause of the original bug:
    //   Input.GetMouseButtonDown / Input.GetTouch are completely separate from Unity's
    //   UI EventSystem. A tap on a Button sets tapped=true here AND fires the button's
    //   onClick — the Input API has no knowledge of what the EventSystem consumed.
    //   EventSystem.IsPointerOverGameObject bridges the gap.
    //
    // Two separate API calls are required:
    //   IsPointerOverGameObject()             — mouse (no argument), for Editor/Standalone
    //   IsPointerOverGameObject(touch.fingerId) — per-finger, for Android/iOS
    //
    // We also loop ALL touches (not just index 0). Only checking GetTouch(0) would
    // miss a UI button pressed as touch[1] while another finger was already on screen.
    private bool IsTapOnGameWorld()
    {
        var es = EventSystem.current;

        // ── Mouse (Editor / Standalone) ──────────────────────────────────────────
        if (Input.GetMouseButtonDown(0))
        {
            // IsPointerOverGameObject() with no argument checks the mouse position.
            if (es != null && es.IsPointerOverGameObject())
            {
                Debug.Log("[BallLauncher] Mouse click blocked — started over UI.");
                return false;
            }
            return true;
        }

        // ── Touch (Android / iOS) ─────────────────────────────────────────────────
        // Loop every active touch. A touch that started on UI is skipped individually;
        // a touch that started on the game world triggers the launch.
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Only care about the frame a finger first makes contact.
            if (touch.phase != TouchPhase.Began) continue;

            // IsPointerOverGameObject(fingerId) checks whether THIS specific finger
            // started over a UI element. Using fingerId (not touch array index i)
            // is critical — fingerId is stable across frames, array index is not.
            if (es != null && es.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log($"[BallLauncher] Touch {touch.fingerId} blocked — started over UI.");
                continue;   // this finger hit UI — check if another finger hit world
            }

            // This finger started on the game world — valid launch tap.
            return true;
        }

        return false;
    }

    void ResetCooldown() => _inputCooldown = false;

    // Called by PauseMenuUI.ResumeAfterDelay() right after GameManager.ResumeGame().
    // Blocks launch input for `seconds` of real (unscaled) time so the touch that
    // pressed Resume cannot reach ThrowYellowBall() in the same or next frame.
    public void BlockInputFor(float seconds)
    {
        _resumeBlockUntil = Time.unscaledTime + seconds;
        Debug.Log($"[BallLauncher] Input blocked until unscaledTime={_resumeBlockUntil:F3} " +
                  $"(+{seconds}s from now={Time.unscaledTime:F3})");
    }

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
            _allThrown      = true;
            _waitingForExit = true;
        }
    }

    void OnAllBallsExited()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing) return;
        if (LevelManager.Instance == null || LevelManager.Instance.TargetsRemaining <= 0) return;

        if (GameManager.Instance.Lives > 0)
        {
            _allThrown = false;
            LevelManager.Instance.RespawnOrbiters();
        }
    }
}
