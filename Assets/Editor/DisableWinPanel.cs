using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class DisableWinPanel
{
    [MenuItem("Tools/Disable Win Panel")]
    static void DisablePanel()
    {
        GameObject winPanel = GameObject.Find("WinPanel");
        if (winPanel != null)
        {
            winPanel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            Debug.Log("WinPanel disabled and scene saved!");
        }
        else
        {
            Debug.LogWarning("WinPanel not found in scene! It may already be inactive (GameObject.Find skips inactive objects). Select it manually in the Hierarchy and uncheck the Active box.");
        }
    }
}
