using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    private static bool _initialized;

    private void Awake()
    {
        if (_initialized)
        {
            Destroy(gameObject);
            return;
        }

        _initialized = true;
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        if (videoPlayer == null)
            videoPlayer = FindFirstObjectByType<VideoPlayer>();

        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        if (videoPlayer != null)
        {
            bool videoEnded = false;
            videoPlayer.loopPointReached += (_) => videoEnded = true;

            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);

            videoPlayer.Play();

            yield return new WaitUntil(() => videoEnded);
        }
        else
        {
            yield return new WaitForSeconds(5f);
        }

        SceneManager.LoadScene(1);
    }
}