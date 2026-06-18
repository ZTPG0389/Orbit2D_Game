using UnityEngine;
using UnityEngine.SceneManagement;

public class SimplePause : MonoBehaviour
{
    public GameObject pausePanel;
    private CanvasGroup _cg;

    void Start()
    {
        if (pausePanel != null)
            _cg = pausePanel.GetComponent<CanvasGroup>();
        HidePanel();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        ShowPanel();
    }

    public void ResumeGame()
    {
        PauseMenuUI.InputBlocked = false;
        PauseMenuUI.IsPaused     = false;
        HidePanel();
        Time.timeScale = 1f;
    }

    // ── Navigation: always resume before any scene change ───────────────────
    // Wire pause panel buttons to these instead of SceneLoader.LoadMainMenu() directly.
    public void GoToMainMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToLevelSelect()
    {
        ResumeGame();
        SceneManager.LoadScene("LevelSelected");
    }

    void ShowPanel()
    {
        if (_cg != null)
        {
            _cg.alpha          = 1f;
            _cg.interactable   = true;
            _cg.blocksRaycasts = true;
        }
        pausePanel.SetActive(true);
    }

    void HidePanel()
    {
        if (pausePanel == null) return;
        if (_cg != null)
        {
            _cg.alpha          = 0f;
            _cg.interactable   = false;
            _cg.blocksRaycasts = false;
        }
        pausePanel.SetActive(false);
    }
}
