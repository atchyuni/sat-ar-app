using UnityEngine;
using TMPro;
using AvatarSDK.MetaPerson.Loader;
using System.Threading.Tasks;

public class AvatarLoader : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private MetaPersonLoader metaPersonLoader;
    [SerializeField] private OrbitControls cameraControls;
    [SerializeField] private TMP_Text titleText;

    [Header("Avatar Positioning")]
    [SerializeField] private Vector3 avatarWorldPosition = new Vector3(-0.6f, 0, 0);

    async void Start()
    {
        // --- 1. get data from [AvatarManager] ---
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;
        string avatarName = AvatarManager.Instance.CurrentAvatarName;

        // --- 2. update title ---
        if (titleText != null && !string.IsNullOrEmpty(avatarName))
        {
            titleText.text = $"++Welcome Back, {avatarName}++";
        }
        else if (titleText != null)
        {
            titleText.text = "++Welcome Back++"; // fallback
        }

        // --- 3. load model ---
        if (metaPersonLoader == null || cameraControls == null)
        {
            Debug.LogError("[MetaPersonLoader] or [CameraControls] not assigned");
            return;
        }

        if (!string.IsNullOrEmpty(avatarUrl))
        {
            Debug.Log($"loading model from: {avatarUrl}");
            bool isLoaded = await metaPersonLoader.LoadModelAsync(avatarUrl);

            if (isLoaded && metaPersonLoader.transform.childCount > 0)
            {
                GameObject avatarObject = metaPersonLoader.transform.GetChild(0).gameObject;
                GameObject avatarPivot = new GameObject("AvatarPivot");
                
                avatarPivot.transform.position = avatarWorldPosition;

                avatarObject.transform.SetParent(avatarPivot.transform);
                avatarObject.transform.localPosition = new Vector3(0, -0.9f, 0);
                avatarPivot.transform.rotation = Quaternion.Euler(0, 180, 0);
                
                cameraControls.target = avatarPivot.transform;
                Debug.Log("[AvatarLoader] successful");
            }
        }
        else
        {
            Debug.LogWarning("missing avatar url in [AvatarManager]");
        }
    }
}