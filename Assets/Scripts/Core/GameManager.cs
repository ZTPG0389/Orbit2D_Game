using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, LevelComplete, GameOver }

    public GameState State        { get; private set; }
    public int       CurrentLevel => _currentLevel;
    public int       Level        => _currentLevel;   // alias kept for HUD/UI callers
    public int       Lives        { get; private set; }

    private int  _currentLevel    = 1;
    private bool _gameOverCalled  = false;

    public event Action<GameState> OnStateChanged;
    public event Action<int>       OnLivesChanged;
    public event Action<int>       OnLevelChanged;

    private const int    StartLives       = 3;
    private const int    GameSceneIndex   = 2;
    private const int    Game2DSceneIndex = 3;
    private const int    MenuSceneIndex   = 1;
    private const string GameScene        = "Game";
    private const string Game2DScene      = "Game2D";
    private const string MenuScene        = "MainMenu";

    private void Awake()
    {
        Debug.Log("[GameManager] Awake in scene: " + SceneManager.GetActiveScene().name);

        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("[GameManager] Start in scene: " + currentScene);

        if (currentScene == GameScene || currentScene == Game2DScene)
        {
            Debug.Log("[GameManager] Start — calling BeginGame (direct Game scene entry)");
            BeginGame();
        }
        else
        {
            SetState(GameState.MainMenu);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[GameManager] OnSceneLoaded: " + scene.name + "  State=" + State);
        if (scene.name == GameScene || scene.name == Game2DScene)
            BeginGame();
        else if (scene.name == MenuScene)
            SetState(GameState.MainMenu);
    }

    private void BeginGame()
    {
        _currentLevel   = 1;
        _gameOverCalled = false;
        Lives = StartLives;
        ScoreManager.Instance?.ResetForNewGame();
        OnLevelChanged?.Invoke(_currentLevel);
        OnLivesChanged?.Invoke(Lives);
        SetState(GameState.Playing);
        LevelManager.Instance?.LoadLevel(_currentLevel);
        Debug.Log("[GameManager] BeginGame — Level=" + _currentLevel + " Lives=" + Lives);
    }

    public void StartGame()    => SceneManager.LoadScene(GameSceneIndex);
    public void RestartGame()  => SceneManager.LoadScene(GameSceneIndex);
    public void GoToMainMenu() => SceneManager.LoadScene(MenuSceneIndex);

    public void PauseGame()
    {
        SetState(GameState.Paused);
        PauseMenuUI.Instance?.Show();
    }

    public void RestartCurrentLevel()
    {
        _gameOverCalled = false;
        Time.timeScale  = 1f;
        Lives = StartLives;
        OnLivesChanged?.Invoke(Lives);
        SetState(GameState.Playing);
        LevelManager.Instance?.LoadLevel(_currentLevel);
        GameOverUI.Instance?.Hide();
    }

    public void LoseLife()
    {
        if (_gameOverCalled) return;
        Lives = Mathf.Max(0, Lives - 1);
        Debug.Log("LoseLife called, lives remaining: " + Lives);
        OnLivesChanged?.Invoke(Lives);
        if (Lives <= 0)
        {
            _gameOverCalled = true;
            SetState(GameState.GameOver);
            GameOverUI.Instance?.Show(
                ScoreManager.Instance?.Score     ?? 0,
                ScoreManager.Instance?.BestScore ?? 0
            );
        }
    }

    public void OnLevelComplete()
    {
        if (State != GameState.Playing) return;
        Time.timeScale = 0;
        SetState(GameState.LevelComplete);
        LevelCompleteUI2D.Instance?.Show(_currentLevel, 500);
    }

    public void AdvanceLevel()
    {
        Time.timeScale = 1f;
        _currentLevel++;
        OnLevelChanged?.Invoke(_currentLevel);
        if (_currentLevel > 15)
        {
            ShowWinPanel();
        }
        else
        {
            SetState(GameState.Playing);
            LevelManager.Instance?.LoadLevel(_currentLevel);
        }
    }

    public void ShowWinPanel()
    {
        _gameOverCalled = false;
        Lives = StartLives;
        _currentLevel = 1;
        SceneManager.LoadScene(Game2DSceneIndex);
    }

    private void SetState(GameState newState)
    {
        State = newState;
        Debug.Log("[GameManager] State -> " + newState + "  Level=" + _currentLevel + "  Lives=" + Lives);
        OnStateChanged?.Invoke(State);
    }
}
