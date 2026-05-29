using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance;

    [SerializeField] CanvasGroup group;
    [SerializeField] Button      resumeBtn;
    [SerializeField] Button      settingsBtn;
    [SerializeField] Button      menuBtn;

    void Awake() => Instance = this;

    void Start()
    {
        Hide();
        resumeBtn.onClick.AddListener(OnResume);
        settingsBtn.onClick.AddListener(OnSettings);
        menuBtn.onClick.AddListener(OnMenu);
    }

    public void Show()
    {
        group.alpha          = 1f;
        group.interactable   = true;
        group.blocksRaycasts = true;
        Time.timeScale       = 0f;

        // Card is always active (never SetActive toggled), so OnEnable won't re-fire.
        // Explicitly trigger the popup animation each time the panel opens.
        var card = transform.Find("Card");
        if (card != null)
            card.GetComponent<CardPopupAnimation>()?.PlayAnimation();
    }

    public void Hide()
    {
        group.alpha          = 0f;
        group.interactable   = false;
        group.blocksRaycasts = false;
    }

    void OnResume()
    {
        Hide();
        Time.timeScale = 1f;
    }

    void OnSettings()
    {
        Hide();
        SettingsUI.Instance.Show();
    }

    void OnMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
