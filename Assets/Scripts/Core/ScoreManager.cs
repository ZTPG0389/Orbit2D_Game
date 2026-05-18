using UnityEngine;
using System;

// Step 25 — Score, combo (x100xN), level bonus (x500), PlayerPrefs best score
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int  Score      { get; private set; }
    public int  BestScore  { get; private set; }
    public int  ComboCount { get; private set; }
    public bool IsNewBest  { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;

    private const string BestScoreKey  = "OrbitDrop_BestScore";
    private const int    HitBasePoints = 100;
    private const int    LevelBonus    = 500;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Playing when GameManager.Instance.Level == 1:
                ResetForNewGame();
                break;
            case GameManager.GameState.LevelComplete:
                AddScore(LevelBonus);
                break;
        }
    }

    // Returns points awarded so callers can display a popup
    public int RegisterHit()
    {
        ComboCount++;
        int points = HitBasePoints * ComboCount;
        AddScore(points);
        OnComboChanged?.Invoke(ComboCount);
        Debug.Log($"[ScoreManager] Hit! Combo={ComboCount}  +{points}pts  Total={Score}");
        return points;
    }

    public void ResetCombo()
    {
        if (ComboCount == 0) return;
        ComboCount = 0;
        OnComboChanged?.Invoke(ComboCount);
    }

    private void AddScore(int points)
    {
        Score += points;
        if (Score > BestScore)
        {
            IsNewBest = true;
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }
        OnScoreChanged?.Invoke(Score);
    }

    public void ResetForNewGame()
    {
        Score      = 0;
        ComboCount = 0;
        IsNewBest  = false;
        OnScoreChanged?.Invoke(Score);
        OnComboChanged?.Invoke(ComboCount);
    }
}
