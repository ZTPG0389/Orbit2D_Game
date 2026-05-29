using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static bridge between Level Select and game scenes.
/// No MonoBehaviour — safe to call from any script in any scene.
///
/// Flow:
///   Level Select  →  PlayLevel(n)           →  Level{n} scene (or Game2D fallback)
///   Game complete →  CompleteLevel(n, stars) →  UnlockNextLevel + SaveStars
///   After unlock  →  ReturnToLevelSelect()   →  LevelSelected scene
/// </summary>
public static class GameProgressManager
{
    // ── Scene names ──────────────────────────────────────────
    const string SceneGame2D      = "Game";
    const string SceneLevelSelect = "LevelSelected";
    const int    TotalLevels      = 50;

    // ── SelectedLevel ────────────────────────────────────────
    // Which level was tapped in Level Select.
    // GameManager.BeginGame() reads this to know where to start.
    public static int SelectedLevel
    {
        get => Mathf.Max(PlayerPrefs.GetInt("SelectedLevel", 1), 1);
        set
        {
            PlayerPrefs.SetInt("SelectedLevel", value);
            PlayerPrefs.Save();
        }
    }

    // ── PlayLevel ────────────────────────────────────────────
    /// <summary>
    /// Called by LevelButton when the player taps a level.
    /// Stores the selection and loads the appropriate scene.
    ///
    /// If "Level{n}" exists in Build Settings it is loaded directly.
    /// Otherwise the game falls back to the shared Game2D scene, and
    /// GameManager.BeginGame() reads SelectedLevel to start at the right level.
    /// </summary>
    public static void PlayLevel(int level)
    {
        Debug.Log($"[GameProgress] Tapped Level {level} → loading scene");
        SelectedLevel = level;

        string levelScene = $"Level{level}";
        if (SceneIsInBuild(levelScene))
            SceneManager.LoadScene(levelScene);
        else
        {
            Debug.LogWarning($"[GameProgress] '{levelScene}' not in Build — using {SceneGame2D}");
            SceneManager.LoadScene(SceneGame2D);
        }
    }

    // ── CompleteLevel ────────────────────────────────────────
    /// <summary>
    /// Call this when a level is beaten.
    /// Saves the star rating and unlocks the next level.
    /// </summary>
    public static void CompleteLevel(int completedLevel, int stars)
    {
        Debug.Log($"[GameProgress] Level {completedLevel} complete — stars={stars}");

        // Persist star rating (keeps best)
        LevelSelectManager.SaveLevelStars(completedLevel, stars);

        // Unlock next level using the exact logic from requirements
        LevelSelectManager.UnlockNextLevel(completedLevel);
    }

    // ── ReturnToLevelSelect ───────────────────────────────────
    /// <summary>
    /// Navigates back to the Level Select screen.
    /// The screen re-reads PlayerPrefs on Start(), so newly unlocked
    /// levels will automatically appear blue and interactive.
    /// </summary>
    public static void ReturnToLevelSelect()
    {
        Debug.Log("[GameProgress] → Returning to Level Select");
        Time.timeScale = 1f;          // reset in case game was paused
        SceneManager.LoadScene(SceneLevelSelect);
    }

    // ── GoToNextLevel ─────────────────────────────────────────
    /// <summary>
    /// Directly advances to the next level without returning to Level Select.
    /// Useful for future "Continue" button on Level Complete panel.
    /// </summary>
    public static void GoToNextLevel()
    {
        int next = SelectedLevel + 1;
        if (next > TotalLevels)
        {
            Debug.Log("[GameProgress] All levels complete — returning to Level Select");
            ReturnToLevelSelect();
        }
        else
        {
            PlayLevel(next);
        }
    }

    // ── Helpers ──────────────────────────────────────────────
    private static bool SceneIsInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
                return true;
        }
        return false;
    }
}
