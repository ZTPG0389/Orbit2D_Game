using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] GameObject  splashScreen;

    private static bool _initialized;

    private void Awake()
    {
        if (_initialized) { Destroy(gameObject); return; }
        _initialized = true;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount  = 0;
    }

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = FindFirstObjectByType<VideoPlayer>();

        // Show splash immediately so screen is never black during preparation
        if (splashScreen != null)
            splashScreen.SetActive(true);

        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(1);
        if (op == null) { yield break; }
        op.allowSceneActivation = false;

        if (videoPlayer != null)
        {
            bool videoEnded = false;
            videoPlayer.loopPointReached += (_) => videoEnded = true;

            // Prepare in background — splashScreen covers the black screen during this
            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);

            // Video is ready — hide splash and play
            if (splashScreen != null)
                splashScreen.SetActive(false);

            videoPlayer.Play();
            yield return new WaitUntil(() => videoEnded);
        }
        else
        {
            yield return new WaitForSeconds(5f);
        }

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
    }

    // When app returns to foreground from a mid-game background, send the
    // player back to MainMenu rather than resuming an unknown game state.
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) return;
        if (SceneManager.GetActiveScene().buildIndex > 1)
            SceneManager.LoadScene(1);
    }
}
