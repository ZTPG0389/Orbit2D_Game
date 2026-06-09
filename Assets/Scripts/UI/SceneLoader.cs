using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGame() { SceneManager.LoadScene(3); }  // Game
    public void LoadMainMenu() { SceneManager.LoadScene(1); }  // MainMenu
    public void LoadLevelSelect() { SceneManager.LoadScene(2); }  // LevelSelected
}