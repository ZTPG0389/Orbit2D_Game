using UnityEngine;
using UnityEngine.SceneManagement;

// Inspector-callable scene navigation — uses build indices for reliability on Android
public class SceneLoader : MonoBehaviour
{
    public void LoadGame()        { SceneManager.LoadScene(2); }           // Game
    public void LoadGame2D()      { SceneManager.LoadScene(3); }           // Game2D
    public void LoadMainMenu()    { SceneManager.LoadScene(1); }           // MainMenu
    public void LoadLevelSelect() { SceneManager.LoadScene("LevelSelected"); } // Level Select
}
