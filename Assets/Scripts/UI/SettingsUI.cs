using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance;

    [SerializeField] CanvasGroup  group;
    [SerializeField] CustomToggle musicToggle;
    [SerializeField] CustomToggle sfxToggle;
    [SerializeField] CustomToggle vibrationToggle;
    [SerializeField] Button       backBtn;

    void Awake() => Instance = this;

    void Start()
    {
        Hide();

        musicToggle.SetValue(PlayerPrefs.GetInt("MusicOn",     1) == 1);
        sfxToggle.SetValue(PlayerPrefs.GetInt("SFXOn",         1) == 1);
        vibrationToggle.SetValue(PlayerPrefs.GetInt("VibrationOn", 1) == 1);

        ApplyMusic(musicToggle.IsOn);
        ApplySFX(sfxToggle.IsOn);

        musicToggle.OnValueChanged     += ApplyMusic;
        sfxToggle.OnValueChanged       += ApplySFX;
        vibrationToggle.OnValueChanged += ApplyVibration;
        backBtn.onClick.AddListener(OnBack);
    }

    void ApplyMusic(bool on)
    {
        PlayerPrefs.SetInt("MusicOn", on ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance?.SetMusicVolume(on ? 0.4f : 0f);
    }

    void ApplySFX(bool on)
    {
        PlayerPrefs.SetInt("SFXOn", on ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance?.SetSFXVolume(on ? 1f : 0f);
    }

    void ApplyVibration(bool on)
    {
        PlayerPrefs.SetInt("VibrationOn", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnBack()
    {
        Hide();
        PauseMenuUI.Instance?.Show();
    }

    public void Show()
    {
        group.alpha          = 1f;
        group.interactable   = true;
        group.blocksRaycasts = true;
    }

    public void Hide()
    {
        group.alpha          = 0f;
        group.interactable   = false;
        group.blocksRaycasts = false;
    }
}
