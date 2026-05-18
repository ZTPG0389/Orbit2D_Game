using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void OnMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
