using UnityEngine;
using UnityEngine.UI;
using TMPro;

// playButton onClick is wired via Inspector to SceneLoader.LoadGame()
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private Button   playButton;

    private void Start()
    {
        RefreshBestScore();
    }

    private void RefreshBestScore()
    {
        if (bestScoreText == null) return;
        int best = ScoreManager.Instance != null
            ? ScoreManager.Instance.BestScore
            : PlayerPrefs.GetInt("OrbitDrop_BestScore", 0);
        bool hasBest = best > 0;
        bestScoreText.gameObject.SetActive(hasBest);
        if (hasBest) bestScoreText.text = $"BEST  {best:N0}";
    }
}
