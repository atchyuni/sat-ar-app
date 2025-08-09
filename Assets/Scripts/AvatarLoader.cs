using UnityEngine;
using TMPro;
using AvatarSDK.MetaPerson.Loader;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class AvatarLoader : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private MetaPersonLoader metaPersonLoader;
    [SerializeField] private SimpleOrbit cameraControls;
    [SerializeField] private TMP_Text nameText;

    [Header("Avatar Positioning")]
    [SerializeField] private Vector3 avatarWorldPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0.6f, -2.2f);

    [Header("Custom Controls")]
    public GameObject customControls;
    public GameObject tipBox;
    public GameObject controlButton;

    async void Start()
    {
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;
        string avatarName = AvatarManager.Instance.CurrentAvatarName;

        // update title
        if (nameText != null && !string.IsNullOrEmpty(avatarName))
        {
            nameText.text = $"++{avatarName}++";
        }

        // load model
        if (metaPersonLoader == null || cameraControls == null)
        {
            Debug.LogError("[MetaPersonLoader] or [CameraControls] not assigned");
            return;
        }

        if (!string.IsNullOrEmpty(avatarUrl))
        {
            Debug.Log($"loading model from: {avatarUrl}");
            bool isLoaded = await metaPersonLoader.LoadModelAsync(avatarUrl);

            if (isLoaded)
            {
                Transform avatarRoot = metaPersonLoader.transform;

                // avatarRoot.position = avatarWorldPosition;
                avatarRoot.rotation = Quaternion.Euler(0, 180, 0);

                cameraControls.transform.position = avatarRoot.position + cameraOffset;
                cameraControls.transform.LookAt(avatarRoot);

                cameraControls.target = avatarRoot;
            }
        }
        else
        {
            Debug.LogWarning("[AvatarManager] missing url");
        }
    }

    public void OnReturnToTitleClicked()
    {
        SceneManager.LoadScene("Startup");
    }

    public void OnControlButtonClick()
    {
        if (customControls != null)
        {
            customControls.SetActive(true);
            controlButton.SetActive(false);
            tipBox.SetActive(false);
        }
    }
}