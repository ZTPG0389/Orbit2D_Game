using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
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
        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        // Pre-load MainMenu in background while splash is visible
        AsyncOperation op = SceneManager.LoadSceneAsync(1);
        if (op == null)
        {
            Debug.LogError("[Bootstrap] LoadSceneAsync(1/MainMenu) returned null — scene missing from build?");
            yield break;
        }
        op.allowSceneActivation = false;

        // Show splash for 2 seconds
        yield return new WaitForSeconds(2f);

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
    }
}
