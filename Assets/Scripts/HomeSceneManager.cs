using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeSceneManager : MonoBehaviour
{
    public void OnCreateAvatarClicked()
    {
        Debug.Log("Create Avatar button clicked");
        SceneManager.LoadScene("AvatarCreation");
        // TODO: add "AvatarCreation" scene to build settings
    }

    public void OnLoadAvatarClicked()
    {
        Debug.Log("Load Avatar button clicked");
        SceneManager.LoadScene("ARTherapy");
        // TODO: add "ARTherapy" scene to build settings
    }

    // public void OnQuitButtonClicked()
    // {
    //     Debug.Log("Quit button clicked");
    //     Application.Quit();
    // }
}
