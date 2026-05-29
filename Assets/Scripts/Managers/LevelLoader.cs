using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    public static void LoadLevel(int levelNumber)
    {
        // Write BOTH keys so GameManager (reads "SelectedLevel") and
        // GameLevelSetup (reads "CurrentLevel") always agree on the level.
        PlayerPrefs.SetInt("CurrentLevel",  levelNumber);
        PlayerPrefs.SetInt("SelectedLevel", levelNumber);
        PlayerPrefs.SetInt("IsGeneratedLevel", levelNumber > 15 ? 1 : 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }
}
