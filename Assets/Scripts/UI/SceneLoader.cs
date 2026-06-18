using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Safety net: subscribes once at domain load and forces timeScale = 1 on
    // every scene load, catching any path that skips explicit pause cleanup.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void RegisterSceneGuard()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  // prevent duplicate on domain reload
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Time.timeScale != 1f)
        {
            Debug.LogWarning($"[SceneLoader] timeScale={Time.timeScale} on '{scene.name}' load — forcing 1.");
            Time.timeScale = 1f;
        }
        PauseMenuUI.IsPaused     = false;
        PauseMenuUI.InputBlocked = false;
    }

    // Primary fix: explicitly clean up pause state before every scene change.
    static void ResetPauseState()
    {
        Time.timeScale           = 1f;
        PauseMenuUI.IsPaused     = false;
        PauseMenuUI.InputBlocked = false;
    }

    public void LoadGame()        { ResetPauseState(); SceneManager.LoadScene(3); }  // Game
    public void LoadMainMenu()    { ResetPauseState(); SceneManager.LoadScene(1); }  // MainMenu
    public void LoadLevelSelect() { ResetPauseState(); SceneManager.LoadScene(2); }  // LevelSelected
}