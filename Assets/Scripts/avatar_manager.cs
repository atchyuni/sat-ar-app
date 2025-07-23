// adapted from sat-master
using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance { get; private set; }

    public string CurrentAvatarUrl { get; private set; }
     

    private void Awake()
    {
        // ensure only one instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // for persisting across scenes
            Debug.Log("[AvatarManager] Instance is set.");
        }
        else
        {
            Debug.Log("[AvatarManager] Another instance of AvatarManager exists. Destroying this one.");
            Destroy(gameObject);
        }
    }

    public void SetCurrentAvatar(string avatarUrl)
    {
        Debug.Log("SetCurrentAvatar called with avatar URL: " + avatarUrl);

        // check for null values
        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("SetCurrentAvatar received null avatar or empty URL.");
            return;
        }
 
        CurrentAvatarUrl = avatarUrl;
        Debug.Log($"AvatarManager: SetCurrentAvatar called with URL: {avatarUrl}");
    }
}
