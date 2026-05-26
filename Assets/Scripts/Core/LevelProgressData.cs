using UnityEngine;

public static class LevelProgressData
{
    private const string KEY_UNLOCKED = "HighestUnlockedLevel";
    private const string KEY_STARS    = "Level_{0}_Stars";

    public static int GetUnlockedLevel()
    {
        return PlayerPrefs.GetInt(KEY_UNLOCKED, 1);
    }

    public static void UnlockNext(int completedLevel)
    {
        int current = GetUnlockedLevel();
        if (completedLevel >= current)
        {
            PlayerPrefs.SetInt(KEY_UNLOCKED, completedLevel + 1);
            PlayerPrefs.Save();
            Debug.Log("[LevelProgress] Unlocked level: " + (completedLevel + 1));
        }
        else
        {
            Debug.Log("[LevelProgress] Level " + (completedLevel + 1) + " already unlocked — no change.");
        }
    }

    public static void SetStars(int level, int stars)
    {
        string key = string.Format(KEY_STARS, level);
        int existing = PlayerPrefs.GetInt(key, 0);
        if (stars > existing)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    public static int GetStars(int level)
    {
        return PlayerPrefs.GetInt(string.Format(KEY_STARS, level), 0);
    }

    // Editor helper — call from a menu item or cheat button to reset all progress
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(KEY_UNLOCKED);
        for (int i = 1; i <= 15; i++)
            PlayerPrefs.DeleteKey(string.Format(KEY_STARS, i));
        PlayerPrefs.Save();
        Debug.Log("[LevelProgress] All progress reset.");
    }
}
