using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class DisplayUI : MonoBehaviour
{
    [SerializeField] private GameObject backgroundOverlay;

    [Header("Save Popup")]
    [SerializeField] private GameObject savePopup;
    [SerializeField] private TMP_InputField nameInput;

    [Header("Share Code Panel")]
    [SerializeField] private GameObject shareCodeDisplay;
    [SerializeField] private TMP_Text shareCodeText;

    [SerializeField] private string serverUrl = "http://ec2-51-20-107-68.eu-north-1.compute.amazonaws.com:5000";
    // [SerializeField] private string serverUrl = "http://10.74.130.118:5000"; // dev mode
    private bool avatarSaved = false;
    private bool popupShown = false;

    public void GoToStartup()
    {
        StartCoroutine(CleanupToStartup());
    }

    private void Start()
    {
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false);
        }
        savePopup.SetActive(false);
        shareCodeDisplay.SetActive(false);

        avatarSaved = false;
        popupShown = false;
    }

    private void OnDestroy()
    {
        if (popupShown && !avatarSaved)
        {
            StartCoroutine(CleanupAvatar());
        }
    }

    public void ShowSavePopup()
    {
        nameInput.text = "";
        popupShown = true;

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(true);
        }
        savePopup.SetActive(true);
    }

    public async void OnConfirmClick()
    {
        string avatar_name = nameInput.text;
        string avatar_url = AvatarManager.Instance.CurrentAvatarUrl;

        if (string.IsNullOrWhiteSpace(avatar_name) || string.IsNullOrEmpty(avatar_url))
        {
            Debug.LogError("[DisplayUI] missing avatar name or url");
            return;
        }

        if (DBManager.Instance == null)
        {
            Debug.LogError("[DB-Error] null instance");
            return;
        }

        savePopup.SetActive(false);
        Debug.Log("[DisplayUI] saving to db...");

        string share_code = await DBManager.Instance.SaveAvatar(avatar_name, avatar_url);

        if (!string.IsNullOrEmpty(share_code))
        {
            avatarSaved = true;

            string confirmation = $"Your unique code for '<b>{avatar_name}</b>' is: <b>{share_code}</b>. Please save it in a secure place so you can load your avatar in the future.";
            shareCodeText.text = confirmation;
            shareCodeDisplay.SetActive(true);
        }
        else
        {
            Debug.LogError("[DisplayUI] failed to save avatar and get share code");
            if (backgroundOverlay != null)
            {
                backgroundOverlay.SetActive(false);
            }
        }
    }

    public void OnCancelSave()
    {
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false);
        }
        savePopup.SetActive(false);

        StartCoroutine(CleanupAvatar());
    }

    private IEnumerator CleanupAvatar()
    {
        if (avatarSaved)
        {
            Debug.Log("[CleanupAvatar] skipping for saved avatar");
            yield break;
        }

        string current_url = AvatarManager.Instance.CurrentAvatarUrl;
        if (string.IsNullOrEmpty(current_url))
        {
            Debug.Log("[CleanupAvatar] no url to clean");
            yield break;
        }

        string filename = ExtractFilename(current_url);
        if (string.IsNullOrEmpty(filename))
        {
            Debug.LogWarning("[CleanupAvatar] could not extract filename to clean");
            yield break;
        }

        Debug.Log($"[CleanupAvatar] {filename}");
        yield return StartCoroutine(DeleteAvatarFile(filename));
    }

    private IEnumerator CleanupToStartup()
    {
        yield return StartCoroutine(CleanupAvatar());
        SceneManager.LoadScene("Startup");
    }

    private string ExtractFilename(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        try
        {
            int last_index = url.LastIndexOf('/');
            if (last_index >= 0 && last_index < url.Length - 1)
            {
                return url.Substring(last_index + 1);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ExtractFilename] {e.Message}");
        }

        return null;
    }

    private IEnumerator DeleteAvatarFile(string filename)
    {
        string deleteUrl = $"{serverUrl}/delete-file/{filename}";
        
        using (UnityWebRequest request = UnityWebRequest.Delete(deleteUrl))
        {
            request.timeout = 10;
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[DisplayUI] successfully deleted: {filename}");
            }
            else if (request.responseCode == 404)
            {
                Debug.Log($"[DisplayUI] avatar file not found (already deleted?): {filename}");
            }
            else
            {
                Debug.LogWarning($"[DisplayUI] failed to delete {filename}: {request.error} (code: {request.responseCode})");
            }
        }
    }
}