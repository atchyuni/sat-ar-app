using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DisplayUI : MonoBehaviour
{
    [SerializeField] private GameObject backgroundOverlay;

    [Header("Save Name Popup")]
    [SerializeField] private GameObject savePopup;
    [SerializeField] private TMP_InputField nameInput;

    [Header("Share Code Display")]
    [SerializeField] private GameObject codeDisplay;
    [SerializeField] private TMP_Text codeText;

    private void Start()
    {
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false);
        }
        savePopup.SetActive(false);
        codeDisplay.SetActive(false);
    }

    public void OnShowSavePopup()
    {
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(true);
        }
        savePopup.SetActive(true);
    }

    public async void OnConfirmSaveClicked()
    {
        string avatarName = nameInput.text;
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;

        if (string.IsNullOrWhiteSpace(avatarName) || string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("missing avatar name or url");
            return;
        }

        savePopup.SetActive(false);
        Debug.Log("saving to supabase...");

        string shareCode = await SupabaseManager.Instance.SaveAvatar(avatarName, avatarUrl);

        if (!string.IsNullOrEmpty(shareCode))
        {
            string confirmationMessage = $"Your unique code for <b>{avatarName}</b> avatar is: <b>{shareCode}</b>.\n\nPlease save it in a secure place so you can load your avatar in the future.";
            codeText.text = confirmationMessage;
            codeDisplay.SetActive(true);
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
    
    public void OnReturnToTitleClicked()
    {
        SceneManager.LoadScene("startup");
    }
}