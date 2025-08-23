using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using AvatarSDK.MetaPerson.Loader;

public class AvatarCleanup : MonoBehaviour
{
    [SerializeField] private List<string> avatarScenes = new List<string> {
        "AvatarDisplay",
        "UserHome",
        "Exercise1",
        "Exercise6"
    };

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!avatarScenes.Contains(scene.name))
        {
            Debug.Log($"[AvatarCleanup] attempting to destroy model in '{scene.name}'");
            DestroyAvatarModel();
        }
    }

    private void DestroyAvatarModel()
    {
        GameObject taggedAvatar = GameObject.FindWithTag("PlayerAvatar");
        if (taggedAvatar != null)
        {
            Debug.Log("[AvatarCleanup] found and destroyed tagged avatar");
            Destroy(taggedAvatar);
            return;
        }

        // --- FALLBACK: clean up loader child (for non-ar scenes) ---
        MetaPersonLoader metaPersonLoader = FindObjectOfType<MetaPersonLoader>();
        if (metaPersonLoader != null && metaPersonLoader.transform.childCount > 0)
        {
            Destroy(metaPersonLoader.transform.GetChild(0).gameObject);
        }
    }
}