using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Step 36 — TMP score/level/lives/targets/combo display; heart Image alpha; event subscriptions
public class GameHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private TMP_Text targetCountText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private Image[]  lifeIcons;

    private void Awake()
    {
        if (lifeIcons == null || lifeIcons.Length == 0)
        {
            Transform parent = transform.Find("LifeIcons");
            if (parent != null)
                lifeIcons = parent.GetComponentsInChildren<Image>();
        }
    }

    private void Start()
    {
        Debug.Log("GameHUD Start, lifeIcons count: " + (lifeIcons != null ? lifeIcons.Length.ToString() : "NULL"));
        Debug.Log("GameManager Instance: " + (GameManager.Instance != null ? "OK" : "NULL"));

        var gm = GameManager.Instance;
        if (gm != null)
        {
            gm.OnStateChanged += OnStateChanged;
            gm.OnLivesChanged += OnLivesChanged;
            gm.OnLevelChanged += OnLevelChanged;
        }

        var sm = ScoreManager.Instance;
        if (sm != null)
        {
            sm.OnScoreChanged += OnScoreChanged;
            sm.OnComboChanged += OnComboChanged;
        }

        var lm = LevelManager.Instance;
        if (lm != null)
            lm.OnTargetsChanged += OnTargetsChanged;

        if (comboText != null) comboText.gameObject.SetActive(false);

        Refresh();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= OnStateChanged;
            GameManager.Instance.OnLivesChanged -= OnLivesChanged;
            GameManager.Instance.OnLevelChanged -= OnLevelChanged;
        }
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
            ScoreManager.Instance.OnComboChanged -= OnComboChanged;
        }
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnTargetsChanged -= OnTargetsChanged;
    }

    private void OnStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
            Refresh();
    }

    private void OnScoreChanged(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString("N0");

        int best = ScoreManager.Instance?.BestScore ?? 0;
        if (bestText != null)
        {
            bestText.gameObject.SetActive(best > 0);
            if (best > 0) bestText.text = $"BEST {best:N0}";
        }
    }

    private void OnComboChanged(int combo) { }

    private void OnLivesChanged(int lives)
    {
        if (lifeIcons == null) return;
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] == null) continue;
            lifeIcons[i].color = i < lives
                ? new Color(1f, 0.27f, 0.27f, 1f)
                : new Color(1f, 1f, 1f, 0.3f);
        }
    }

    private void OnLevelChanged(int level)
    {
        if (levelText != null) levelText.text = $"LEVEL {level}";
    }

    private void OnTargetsChanged(int remaining)
    {
        if (targetCountText != null) targetCountText.text = remaining.ToString();
    }

    private void Refresh()
    {
        var gm = GameManager.Instance;
        var sm = ScoreManager.Instance;
        var lm = LevelManager.Instance;

        if (gm != null)
        {
            OnLivesChanged(gm.Lives);
            OnLevelChanged(gm.Level);
        }
        if (sm != null)
        {
            OnScoreChanged(sm.Score);
            OnComboChanged(sm.ComboCount);
        }
        if (lm != null)
            OnTargetsChanged(lm.TargetsRemaining);
    }
}
