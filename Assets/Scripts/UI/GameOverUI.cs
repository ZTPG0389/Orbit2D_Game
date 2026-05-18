using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [SerializeField] private TMP_Text    scoreText;
    [SerializeField] private TMP_Text    bestText;
    [SerializeField] private Button      retryBtn;
    [SerializeField] private Button      menuBtn;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private GameObject  panelRoot;

    private void Awake()
    {
        Instance = this;
        SetGroupVisible(false);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
        if (retryBtn != null) retryBtn.onClick.AddListener(OnRetry);
        if (menuBtn  != null) menuBtn.onClick.AddListener(OnMenu);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
        if (retryBtn != null) retryBtn.onClick.RemoveListener(OnRetry);
        if (menuBtn  != null) menuBtn.onClick.RemoveListener(OnMenu);
    }

    private bool _isShowing = false;

    private void OnStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
        {
            Show(
                ScoreManager.Instance?.Score     ?? 0,
                ScoreManager.Instance?.BestScore ?? 0
            );
        }
        else
        {
            Hide();
        }
    }

    public void Show(int score, int best)
    {
        if (_isShowing) return;
        _isShowing = true;

        if (panelRoot != null) panelRoot.SetActive(true);

        if (scoreText != null) scoreText.text = score.ToString("N0");

        if (bestText != null)
        {
            bestText.gameObject.SetActive(true);
            if (score > 0 && score >= best)
            {
                bestText.text  = "\U0001F3C6 NEW BEST!";
                bestText.color = new Color(0.976f, 0.792f, 0.141f, 1f);
            }
            else
            {
                bestText.text  = $"\U0001F3C6 BEST: {best:N0}";
                bestText.color = Color.white;
            }
        }

        if (group != null)
        {
            group.alpha          = 1f;
            group.interactable   = true;
            group.blocksRaycasts = true;
        }
    }

    public void Hide()
    {
        _isShowing = false;
        StopAllCoroutines();
        SetGroupVisible(false);
    }

    private void SetGroupVisible(bool visible)
    {
        if (group == null) return;
        group.alpha          = visible ? 1f : 0f;
        group.interactable   = visible;
        group.blocksRaycasts = visible;
    }

    public void OnRetry() => GameManager.Instance?.RestartCurrentLevel();
    public void OnMenu()  => GameManager.Instance?.GoToMainMenu();
}
