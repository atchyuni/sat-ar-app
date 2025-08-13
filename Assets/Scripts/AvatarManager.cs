// adapted from sat-master
using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance { get; private set; }
    public string CurrentAvatarUrl { get; private set; }
    public string CurrentAvatarName { get; private set; }
    public int CurrentDaysCompleted { get; private set; }

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

    public void SetCurrentAvatar(string avatarUrl, string avatarName, int daysCompleted)
    {
        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("[SetCurrentAvatar] null avatar or empty url");
            return;
        }

        CurrentAvatarUrl = avatarUrl;
        CurrentAvatarName = avatarName;
        CurrentDaysCompleted = daysCompleted;
        Debug.Log($"[AvatarManager] SetCurrentAvatar called with: {avatarUrl}");
    }
    
    // overload for "create avatar" flow, which starts at 0 days
    public void SetCurrentAvatar(string avatarUrl, string avatarName)
    {
        SetCurrentAvatar(avatarUrl, avatarName, 0);
    }
}
