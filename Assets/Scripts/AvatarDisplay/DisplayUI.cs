using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DisplayUI : MonoBehaviour
{
    [SerializeField] private GameObject backgroundOverlay;

    [Header("Save Popup")]
    [SerializeField] private GameObject savePopup;
    [SerializeField] private TMP_InputField nameInput;

    [Header("Share Code Panel")]
    [SerializeField] private GameObject shareCodeDisplay;
    [SerializeField] private TMP_Text shareCodeText;

    public void GoToStartup()
    {
        SceneManager.LoadScene("Startup");
    }

    private void Start()
    {
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false);
        }
        savePopup.SetActive(false);
        shareCodeDisplay.SetActive(false);
    }

    public void ShowSavePopup()
    {
        nameInput.text = "";

        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(true);
        }
        savePopup.SetActive(true);
    }

    public async void OnConfirmClick()
    {
        string avatarName = nameInput.text;
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;

        if (string.IsNullOrWhiteSpace(avatarName) || string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("missing avatar name or url");
            return;
        }

        if (DBManager.Instance == null)
        {
            Debug.LogError("FATAL ERROR: db instance not found");
            return;
        }

        savePopup.SetActive(false);
        Debug.Log("saving to supabase...");

        string shareCode = await DBManager.Instance.SaveAvatar(avatarName, avatarUrl);

        if (!string.IsNullOrEmpty(shareCode))
        {
            string confirmationMessage = $"Your unique code for '<b>{avatarName}</b>' is: <b>{shareCode}</b>.\n\nPlease save it in a secure place so you can load your avatar in the future.";
            shareCodeText.text = confirmationMessage;
            shareCodeDisplay.SetActive(true);
        }
        else
        {
            Debug.LogError("failed to save avatar and get share code");
            // TODO: error message panel display
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
    }
}