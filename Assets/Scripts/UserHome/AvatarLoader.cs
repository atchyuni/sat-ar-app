using UnityEngine;
using TMPro;
using AvatarSDK.MetaPerson.Loader;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEngine.UI;


public class AvatarLoader : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private MetaPersonLoader avatarLoader;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private ProgressBarUI progressBar;
    [SerializeField] private TMP_Text quoteText;
    [SerializeField] private ChecklistManager checklistManager;
    
    [Header("Expression Systems")]
    public RuntimeAnimatorController controller;
    [SerializeField] private AnimationManager animationManager;
    [SerializeField] private FacialSwitcher facialSwitcher;

    [Header("UI References")]
    [SerializeField] private Toggle happyFace;
    [SerializeField] private Toggle sadFace;
    [SerializeField] private Toggle happyBody;
    [SerializeField] private Toggle sadBody;

    [Header("Emotion Controls")]
    public GameObject emotionControls;
    public GameObject tipBox;
    public GameObject emotionButton;

    private bool initialised = false;

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
        string avatar_name = AvatarManager.Instance.CurrentAvatarName;
        int days_completed = AvatarManager.Instance.CurrentDaysCompleted;

        // --- UI UPDATE ---
        DisplayRandomQuote();

        if (nameText != null && !string.IsNullOrEmpty(avatar_name))
        {
            nameText.text = $"{avatar_name}";
        }

        if (progressBar != null)
        {
            progressBar.UpdateProgress(days_completed);
        }

        // --- WELLNESS CHECK ---
        string today_date = DateTime.UtcNow.Date.ToString();
        string checklist_date = PlayerPrefs.GetString("LastChecklistDate", "");

        if (checklist_date != today_date)
        {
            Debug.Log("[AvatarLoader] pending wellness check");
            
            checklistManager.ShowChecklist(() =>
            {
                PlayerPrefs.SetString("LastChecklistDate", today_date);
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
        string avatar_url = AvatarManager.Instance.CurrentAvatarUrl;

        if (string.IsNullOrEmpty(avatar_url))
        {
            Debug.LogWarning("[AvatarManager] missing url");
            return;
        }

        // --- LOAD MODEL ---
        Debug.Log($"[LoadAvatarModel] with: {avatar_url}");
        bool loaded = await avatarLoader.LoadModelAsync(avatar_url);

        if (loaded && avatarLoader.transform.childCount > 0)
        {
            GameObject avatar = avatarLoader.transform.GetChild(0).gameObject;
            avatar.transform.rotation = Quaternion.Euler(0, 180, 0);

            Animator animator = avatar.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                // assign custom controller & bind animator
                animator.runtimeAnimatorController = controller;
                animationManager.animationSwitcher.Animator = animator;
                animationManager.animationSwitcher.SetupOverrideController();
            }
            else
            {
                Debug.LogError("[AvatarLoader] animator not found on loaded avatar");
            }
            
            facialSwitcher.Avatar = avatar;
            animator.gameObject.AddComponent<AnimEventReceiver>().ans = animationManager.animationSwitcher;

            SetIdleFace();
            SetIdleBody();
            initialised = true;
            Debug.Log("[AvatarLoader] systems initialised successfully");
        }
    }

    // --- UI METHODS ---
    public void HappyFaceToggle(bool on) { if (on && initialised) facialSwitcher.SetHappy(); }
    public void SadFaceToggle(bool on) { if (on && initialised) facialSwitcher.SetSad(); }
    public void SetIdleFace() {
        if (!initialised) return;
        if(happyFace) happyFace.isOn = false;
        if(sadFace) sadFace.isOn = false;
        facialSwitcher.SetIdle();
    }

    public void HappyBodyToggle(bool on) { if (on && initialised) animationManager.Happy(); }
    public void SadBodyToggle(bool on) { if (on && initialised) animationManager.Cry(); }
    public void SetIdleBody() {
        if (!initialised) return;
        if(happyBody) happyBody.isOn = false;
        if(sadBody) sadBody.isOn = false;
        animationManager.SetIdle();
    }

    private void DisplayRandomQuote()
    {
        if (quoteText != null && quotes.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, quotes.Count);
            quoteText.text = $"{quotes[randomIndex]}";
        }
    }

    public void OnReturnClick()
    {
        SceneManager.LoadScene("Startup");
    }

    public void OnEmotionClick()
    {
        if (emotionControls != null)
        {
            emotionControls.SetActive(true);
            emotionButton.SetActive(false);
            tipBox.SetActive(false);
        }
    }
}