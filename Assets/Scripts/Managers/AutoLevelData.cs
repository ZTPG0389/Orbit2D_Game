using UnityEngine;

public struct AutoLevelConfig
{
    public float ballSpeed;
    public int   obstacleCount;
    public float obstacleSpeed;
    public float spawnInterval;
    public int   targetScore;
}

public static class AutoLevelData
{
    public static AutoLevelConfig GenerateLevel(int levelNum)
    {
        int n = levelNum - 15; // offset from last manual level

        return new AutoLevelConfig
        {
            ballSpeed     = Mathf.Min(3f + n * 0.2f,  8f),
            obstacleCount = 3 + n / 3,
            obstacleSpeed = Mathf.Min(2f + n * 0.15f, 6f),
            spawnInterval = Mathf.Max(0.5f, 2f - n * 0.05f),
            targetScore   = 100 + n * 50,
        };
    }
}
