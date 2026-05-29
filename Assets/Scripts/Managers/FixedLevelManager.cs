using UnityEngine;

public static class FixedLevelManager
{
    const int TotalLevels = 50;

    public static void CompleteLevel(int levelNum)
    {
        if (levelNum >= TotalLevels)
        {
            Debug.Log("All 50 levels complete! Restarting from Level 1");
            PlayerPrefs.SetInt("CurrentLevel",    1);
            PlayerPrefs.SetInt("MaxUnlockedLevel", TotalLevels);
        }
        else
        {
            int next = levelNum + 1;
            PlayerPrefs.SetInt("CurrentLevel", next);

            int max = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
            if (next > max)
                PlayerPrefs.SetInt("MaxUnlockedLevel", next);
        }

        PlayerPrefs.Save();
    }

    public static int GetCurrentLevel()  => PlayerPrefs.GetInt("CurrentLevel",     1);
    public static int GetMaxUnlocked()   => PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
    public static bool IsUnlocked(int n) => n <= GetMaxUnlocked();
    public static int GetTotalLevels()   => TotalLevels;
}
