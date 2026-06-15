using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject  splashImage;   // full-screen cover shown while video prepares

    private static bool _initialized;

    private void Awake()
    {
        if (_initialized) { Destroy(gameObject); return; }
        _initialized = true;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = FindFirstObjectByType<VideoPlayer>();

        // Cover the black RenderTexture immediately — visible before any video frame arrives
        if (splashImage != null)
            splashImage.SetActive(true);

        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        if (videoPlayer != null)
        {
            bool videoEnded = false;
            videoPlayer.loopPointReached += (_) => videoEnded = true;

            // Prepare fills the decode pipeline so Play() outputs frames immediately
            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);

            videoPlayer.Play();

            // Wait until the first real frame is written into the RenderTexture.
            // frame == 0 means the texture is still empty; frame >= 1 means pixels are there.
            yield return new WaitUntil(() => videoPlayer.frame >= 1);

            // RenderTexture now has valid content — safe to hide the splash cover
            if (splashImage != null)
                splashImage.SetActive(false);

            yield return new WaitUntil(() => videoEnded);
        }
        else
        {
            if (splashImage != null)
                splashImage.SetActive(false);
            yield return new WaitForSeconds(5f);
        }

        SceneManager.LoadScene(1);
    }
}
