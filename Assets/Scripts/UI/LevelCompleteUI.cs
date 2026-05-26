using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// DISABLED — WinPanel (LevelCompleteUI2D) is the active level complete handler.
// This script is kept to avoid missing-script warnings on LevelCompletePanel.
public class LevelCompleteUI : MonoBehaviour
{
    [SerializeField] private TMP_Text    levelText;
    [SerializeField] private TMP_Text    bonusText;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private GameObject  panelRoot;

    private void Awake()
    {
        // Permanently hide LevelCompletePanel at scene start
        if (panelRoot != null) panelRoot.SetActive(false);
        if (group != null)
        {
            group.alpha          = 0f;
            group.interactable   = false;
            group.blocksRaycasts = false;
        }
    }

    // Event subscription removed — this script no longer reacts to GameState changes
    private void Start()  { }
    private void OnDestroy() { }
}
