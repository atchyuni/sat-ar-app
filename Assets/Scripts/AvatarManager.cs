// adapted from sat-master
using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance { get; private set; }
    public string CurrentAvatarUrl { get; private set; }
    public string CurrentAvatarName { get; private set; }

    private void Awake()
    {
        // ensure only one instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[AvatarManager] set");
        }
        else
        {
            Debug.Log("[AvatarManager] instance exists, destroying this one...");
            Destroy(gameObject);
        }
    }

    public void SetCurrentAvatar(string avatarUrl, string avatarName)
    {
        Debug.Log("SetCurrentAvatar called with: " + avatarUrl);

        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("SetCurrentAvatar received null avatar or empty url");
            return;
        }
 
        CurrentAvatarUrl = avatarUrl;
        CurrentAvatarName = avatarName;
        Debug.Log($"[AvatarManager] SetCurrentAvatar called with: {avatarUrl}");
    }
}
