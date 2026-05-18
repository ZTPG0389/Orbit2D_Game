using UnityEngine;

public class RetryButton : MonoBehaviour
{
    public void OnRetryClicked()
    {
        GameManager.Instance?.RestartCurrentLevel();
    }
}
