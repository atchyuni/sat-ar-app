using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigationHandler : MonoBehaviour
{
    public void GoToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("ERROR: [NavigationHandler] empty scene name");
            return;
        }

        Debug.Log($"[SceneNavigation]: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}