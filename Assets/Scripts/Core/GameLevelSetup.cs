using UnityEngine;

// Attach to any persistent GameObject in the Game scene.
// Reads PlayerPrefs written by LevelLoader and applies the level config on Start.
public class GameLevelSetup : MonoBehaviour
{
    void Start()
    {
        int level       = PlayerPrefs.GetInt("CurrentLevel",     1);
        int isGenerated = PlayerPrefs.GetInt("IsGeneratedLevel", 0);

        if (isGenerated == 1)
            ApplyGeneratedLevel(level);
        else
            ApplyManualLevel(level);
    }

    void ApplyGeneratedLevel(int level)
    {
        AutoLevelConfig cfg = AutoLevelData.GenerateLevel(level);

        // ── Wire managers here as your game grows ────────────────────────────
        // To set targetScore on LevelCompleteUI2D, first make the field public:
        //   public int targetScore = 1000;
        // Then:
        //   var ui = FindFirstObjectByType<LevelCompleteUI2D>();
        //   if (ui != null) ui.targetScore = cfg.targetScore;
        // Example (add public fields / methods to those classes first):
        //   var spawner = FindFirstObjectByType<ObstacleSpawner>();
        //   if (spawner != null) {
        //       spawner.obstacleCount   = cfg.obstacleCount;
        //       spawner.obstacleSpeed   = cfg.obstacleSpeed;
        //       spawner.spawnInterval   = cfg.spawnInterval;
        //   }
        //   var launcher = FindFirstObjectByType<BallLauncher2D>();
        //   if (launcher != null)
        //       launcher.launchSpeed = cfg.ballSpeed;

        Debug.Log($"[GameLevelSetup] Generated level {level} — " +
                  $"ballSpeed={cfg.ballSpeed:F1}  obstacles={cfg.obstacleCount}  " +
                  $"obstacleSpeed={cfg.obstacleSpeed:F1}  spawnInterval={cfg.spawnInterval:F2}  " +
                  $"targetScore={cfg.targetScore}");
    }

    void ApplyManualLevel(int level)
    {
        // Manual levels 1-15 are configured via the scene / Inspector.
        // If LevelManager needs to know the index, call it here.
        if (LevelManager.Instance != null)
            LevelManager.Instance.LoadLevel(level);

        Debug.Log($"[GameLevelSetup] Manual level {level}");
    }
}
