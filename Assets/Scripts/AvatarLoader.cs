using UnityEngine;
using TMPro;
using AvatarSDK.MetaPerson.Loader;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class AvatarLoader : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private MetaPersonLoader metaPersonLoader;
    [SerializeField] private SimpleOrbit cameraControls;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private ProgressBarUI progressBar;
    [SerializeField] private TMP_Text quoteText;
    [SerializeField] private ChecklistManager checklistManager;

    [Header("Avatar Positioning")]
    [SerializeField] private Vector3 avatarWorldPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0.6f, -2.2f);

    [Header("Custom Controls")]
    public GameObject customControls;
    public GameObject tipBox;
    public GameObject controlButton;

    private List<string> quotes = new List<string>
    {
        "Believe you can",
        "You can do this",
        "Every small step counts",
        "You are doing great work",
    };

    void Start()
    {
        // --- RETRIEVE DATA ---
        string avatarName = AvatarManager.Instance.CurrentAvatarName;
        int daysCompleted = AvatarManager.Instance.CurrentDaysCompleted;

        // --- UI UPDATE ---
        DisplayRandomQuote();

        if (nameText != null && !string.IsNullOrEmpty(avatarName))
        {
            nameText.text = $"{avatarName}";
        }

        if (progressBar != null)
        {
            progressBar.UpdateProgress(daysCompleted);
        }

        // --- WELLNESS CHECK ---
        string todayDateString = DateTime.UtcNow.Date.ToString();
        string lastChecklistDate = PlayerPrefs.GetString("LastChecklistDate", "");

        if (lastChecklistDate != todayDateString)
        {
            Debug.Log("[AvatarLoader] pending wellness check");
            
            checklistManager.ShowChecklist(() =>
            {
                PlayerPrefs.SetString("LastChecklistDate", todayDateString);
                PlayerPrefs.Save();
                LoadAvatarModel(); // only after checklist success
            });
        }
        else
        {
            Debug.Log("[AvatarLoader] wellness check already completed");
            LoadAvatarModel();
        }
    }

    private async void LoadAvatarModel()
    {
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;

        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogWarning("[AvatarManager] missing url");
            return;
        }

        Debug.Log($"[LoadAvatarModel]: {avatarUrl}");
        bool isLoaded = await metaPersonLoader.LoadModelAsync(avatarUrl);

        if (isLoaded)
        {
            Transform avatarRoot = metaPersonLoader.transform;
            avatarRoot.rotation = Quaternion.Euler(0, 180, 0);
            cameraControls.transform.position = avatarRoot.position + cameraOffset;
            cameraControls.transform.LookAt(avatarRoot);
            cameraControls.target = avatarRoot;
        }
    }

    private void DisplayRandomQuote()
    {
        if (quoteText != null && quotes.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, quotes.Count);
            quoteText.text = $"[ {quotes[randomIndex]} ]";
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