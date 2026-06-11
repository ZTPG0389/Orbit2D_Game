using UnityEngine;

// Step 34 — SFX enum (Launch/Hit/Miss/LevelComplete); PlaySFX(); PlayBGM(); volume in PlayerPrefs
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // sfxClips array in Inspector must match this order exactly:
    // [0] Launch  [1] Hit  [2] Miss  [3] LevelComplete
    // [4] MissileAttack  [5] Alert  [6] Warning
    public enum SFX { Launch, Hit, Miss, LevelComplete, MissileAttack, Alert, Warning }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip[] sfxClips;  // indices must match SFX enum order
    [SerializeField] private AudioClip   bgmClip;

    private const string SfxVolKey = "OrbitDestroyer_SFXVol";
    private const string BgmVolKey = "OrbitDestroyer_BGMVol";

    public float SfxVolume { get; private set; }
    public float BgmVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SfxVolume = PlayerPrefs.GetFloat(SfxVolKey, 1.0f);
        BgmVolume = PlayerPrefs.GetFloat(BgmVolKey, 0.6f);

        if (sfxSource != null) sfxSource.volume = PlayerPrefs.GetInt("SFXOn", 1) == 1 ? 1f : 0f;
        bool musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        if (bgmSource != null)
        {
            bgmSource.loop   = true;
            bgmSource.volume = musicOn ? 0.4f : 0f;
            if (!musicOn) bgmSource.Stop();
        }
    }

    private void Start()
    {
        PlayBGM();

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.LevelComplete)
            PlaySFX(SFX.LevelComplete);
        else if (state == GameManager.GameState.GameOver)
            PlaySFX(SFX.Miss);
    }

    public void PlaySFX(SFX sfx)
    {
        if (sfxSource == null || sfxClips == null) return;
        int idx = (int)sfx;
        if (idx < 0 || idx >= sfxClips.Length || sfxClips[idx] == null) return;
        sfxSource.PlayOneShot(sfxClips[idx], SfxVolume);
    }

    public void PlayBGM()
    {
        if (bgmSource == null || bgmClip == null) return;
        if (PlayerPrefs.GetInt("MusicOn", 1) == 0) return;
        if (bgmSource.isPlaying && bgmSource.clip == bgmClip) return;
        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource?.Stop();

    public void SetMusicVolume(float v)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = v;
            if (v <= 0f)
                bgmSource.Stop();
            else if (!bgmSource.isPlaying)
                bgmSource.Play();
        }
    }
    public void SetSFXVolume(float v)   { if (sfxSource != null) sfxSource.volume  = v; }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = SfxVolume;
        PlayerPrefs.SetFloat(SfxVolKey, SfxVolume);
        PlayerPrefs.Save();
    }

    public void SetBgmVolume(float volume)
    {
        BgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = BgmVolume;
        PlayerPrefs.SetFloat(BgmVolKey, BgmVolume);
        PlayerPrefs.Save();
    }
}
