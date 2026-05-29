using UnityEngine;

public static class PlayerProgressManager
{
    const string PrefKey = "MaxUnlockedLevel";

    public static int GetMaxUnlocked()
    {
        return PlayerPrefs.GetInt(PrefKey, 1);
    }

    public static void CompleteLevel(int n)
    {
        int current = GetMaxUnlocked();
        if (n + 1 > current)
        {
            PlayerPrefs.SetInt(PrefKey, n + 1);
            PlayerPrefs.Save();
        }
    }

    public static bool IsUnlocked(int n)
    {
        return n <= GetMaxUnlocked();
    }

    // Always show at least 15 buttons; beyond that, show 3 locked ahead of max
    public static int GetTotalLevelsToShow()
    {
        return Mathf.Max(15, GetMaxUnlocked() + 3);
    }
}
