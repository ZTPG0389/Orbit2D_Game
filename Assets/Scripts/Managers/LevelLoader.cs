using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    public static void LoadLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("CurrentLevel", levelNumber);

        if (levelNumber > 15)
        {
            PlayerPrefs.SetInt("IsGeneratedLevel", 1);
        }
        else
        {
            PlayerPrefs.SetInt("IsGeneratedLevel", 0);
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
    }
}
