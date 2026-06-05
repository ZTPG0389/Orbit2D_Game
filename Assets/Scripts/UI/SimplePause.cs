using UnityEngine;

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
