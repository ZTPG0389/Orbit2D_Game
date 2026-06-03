using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    // Blocks Release() and BallLauncher2D from accepting any tap while true.
    // Set to true the moment Resume is tapped (before any yield).
    // Set to false only after the post-resume input-drain window expires.
    public static bool InputBlocked = false;

    // Blocks gameplay while the pause panel is visible.
    // Set to true in Show() and false in ResumeAfterDelay() once time is restored.
    public static bool IsPaused = false;

    [SerializeField] CanvasGroup        group;
    [SerializeField] Button             resumeBtn;
    [SerializeField] Button             settingsBtn;
    [SerializeField] Button             menuBtn;
    [SerializeField] CardPopupAnimation cardPopup;

    private bool _isResuming;

    void Awake() => Instance = this;

    // Defensive: catches any path that makes the panel visible without calling Show().
    void Update()
    {
        if (group != null && group.alpha >= 0.9f && Time.timeScale > 0f)
        {
            Debug.LogWarning("[PauseMenuUI] Panel visible with timeScale=" + Time.timeScale +
                             " — forcing pause. Wire HUD pause button to GameManager.PauseGame().");
            Time.timeScale = 0f;
            IsPaused       = true;
        }
    }

    void Start()
    {
        Hide();

        Debug.Log($"[PauseMenuUI] Start — resumeBtn persistent listeners: " +
                  $"{resumeBtn.onClick.GetPersistentEventCount()} " +
                  $"| settingsBtn: {settingsBtn.onClick.GetPersistentEventCount()} " +
                  $"| menuBtn: {menuBtn.onClick.GetPersistentEventCount()}");

        resumeBtn  .onClick.RemoveAllListeners();
        settingsBtn.onClick.RemoveAllListeners();
        menuBtn    .onClick.RemoveAllListeners();

        resumeBtn  .onClick.AddListener(OnResume);
        settingsBtn.onClick.AddListener(OnSettings);
        menuBtn    .onClick.AddListener(OnMenu);
    }

    // ── Pause (STEP 1) ───────────────────────────────────────────────────────────
    public void Show()
    {
        Debug.Log($"[PauseMenuUI] Show — timeScale BEFORE: {Time.timeScale}  " +
                  $"GameState={GameManager.Instance?.State}");

        group.alpha          = 1f;
        group.interactable   = true;
        group.blocksRaycasts = true;

        IsPaused       = true;
        Time.timeScale = 0f;

        // Clear both flags in case we re-pause mid-resume.
        InputBlocked = false;
        _isResuming  = false;

        Debug.Log($"[PauseMenuUI] Show — timeScale AFTER: {Time.timeScale}  IsPaused={IsPaused}");

        cardPopup?.PlayAnimation();
    }

    public void Hide()
    {
        group.alpha          = 0f;
        group.interactable   = false;
        group.blocksRaycasts = false;
    }

    // ── Resume sequence (STEPS 2 – 5) ────────────────────────────────────────────
    void OnResume()
    {
        if (_isResuming)
        {
            Debug.LogWarning("[PauseMenuUI] OnResume — already resuming, duplicate ignored.");
            return;
        }

        Debug.Log($"[PauseMenuUI] OnResume — timeScale: {Time.timeScale}  " +
                  $"IsPaused={IsPaused}  InputBlocked={InputBlocked}");

        _isResuming = true;

        // Block input IMMEDIATELY — before any yield — so the touch that
        // activated this button cannot reach Release() or BallLauncher2D.
        InputBlocked = true;

        Hide();   // panel gone while timeScale is still 0

        StartCoroutine(ResumeAfterDelay());
    }

    IEnumerator ResumeAfterDelay()
    {
        // ── STEP 2: wait for the panel to visually disappear ─────────────────────
        // WaitForSecondsRealtime works at timeScale = 0.
        // WaitForSeconds and yield return null stall at timeScale = 0.
        Debug.Log("[PauseMenuUI] Step 2 — waiting 0.15 s for panel to clear");
        yield return new WaitForSecondsRealtime(0.15f);

        // ── STEP 3: restore time and clear the pause gate ────────────────────────
        // InputBlocked is still true here — Release() stays blocked even though
        // IsPaused is about to become false.
        Time.timeScale = 1f;
        IsPaused       = false;
        Debug.Log($"[PauseMenuUI] Step 3 — timeScale={Time.timeScale}  " +
                  $"IsPaused={IsPaused}  InputBlocked={InputBlocked}");

        // Restore GameManager.State → Playing so OrbitController2D and
        // BallLauncher2D Update() guards unblock. BallLauncher2D may now
        // detect a tap, but Release() will still reject it via InputBlocked.
        GameManager.Instance?.ResumeGame();

        // Block BallLauncher2D launch input for 0.2 s so the touch that activated
        // the Resume button cannot reach ThrowYellowBall() once State = Playing.
        BallLauncher2D launcher = FindObjectOfType<BallLauncher2D>();
        launcher?.BlockInputFor(0.3f);

        // ── STEP 4: drain the resume-touch from the Input system ─────────────────
        // The touch that triggered the Resume button can remain in Input for
        // several frames. This window absorbs it completely.
        Debug.Log("[PauseMenuUI] Step 4 — draining resume-touch for 0.15 s");
        yield return new WaitForSecondsRealtime(0.15f);

        // ── STEP 5: open input to the player ─────────────────────────────────────
        InputBlocked = false;
        _isResuming  = false;
        Debug.Log($"[PauseMenuUI] Step 5 — InputBlocked={InputBlocked}  ready for new tap");
    }

    void OnSettings()
    {
        Hide();
        SettingsUI.Instance.Show();
    }

    void OnMenu()
    {
        Time.timeScale = 1f;
        IsPaused       = false;
        InputBlocked   = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
