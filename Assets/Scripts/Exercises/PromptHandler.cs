using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GLTFast;
using TMPro;

public class PromptHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject instructionBox;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject debriefPopup;
    [SerializeField] private GameObject backgroundOverlay;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject promptButton;
    [SerializeField] private GameObject endButton;

    // from gpt
    private List<string> promptList = new List<string>
    {
        "Should marriage be defined traditionally or be inclusive of all genders/orientations?",
        "Do current policies adequately address racial (systemic) inequality, or are deeper reforms needed?",
        "Should countries have stricter borders or more open pathways for migrants and refugees?",
        "Where should society draw the line between protecting freedom of speech and preventing harm?",
        "Should governments prioritise aggressive climate policies even if they impact economies, or balance them with growth?",
        "Should healthcare be considered a human right provided by the government, or remain a private responsibility?",
        "Should gender self-identification be fully recognised legally, or should it have certain restrictions?",
        "Should abortion be fully accessible as a reproductive right, or restricted to protect unborn life?",
        "Is capital punishment a just consequence for severe crimes, or an unethical practice that should be abolished?",
        "Should recreational drugs (like cannabis) be legalised, or should they remain controlled substances?",
        "Should governments and tech companies prioritise individual privacy, or surveillance for safety?",
        "Is affirmative action a fair way to correct inequality, or does it create reverse discrimination?",
        "Should animal testing and factory farming be banned, or are they necessary for progress and food supply?",
        "Should harmful or misleading content be removed online, or is that a violation of free speech?",
        "Should governments tax the wealthy more to redistribute resources, or does that discourage success?",
        "Should AI be tightly regulated for ethics and safety, or allowed to innovate freely?",
        "Should countries embrace globalisation, or prioritise national sovereignty and local economies?"
    };

    private List<string> availablePrompts;

    void Awake()
    {
        CleanupRogues();

        if (instructionBox != null) instructionBox.SetActive(true);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);
        if (menuButton != null) menuButton.SetActive(true);
        menuButton.GetComponent<Button>().interactable = false;
        if (menuPanel != null) menuPanel.SetActive(false);
        if (endButton != null) endButton.SetActive(true);
        endButton.GetComponent<Button>().interactable = false;
    }

    private void CleanupRogues()
    {
        var avatars = FindObjectsOfType<TimeBudgetPerFrameDeferAgent>(true);

        if (avatars.Length > 0)
        {
            Debug.LogWarning($"[PromptHandler] found {avatars.Length} rogue glTF model(s), destroying");
            foreach (var avatarComponent in avatars)
            {
                Destroy(avatarComponent.gameObject);
            }
        }
    }

    public void HideInstructions()
    {
        if (instructionBox != null)
        {
            instructionBox.SetActive(false);
            backgroundOverlay.SetActive(false);
            menuButton.GetComponent<Button>().interactable = true;
            endButton.GetComponent<Button>().interactable = true;
        }
    }

    public void OnMenuClick()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            menuButton.SetActive(false);
            backgroundOverlay.SetActive(true);
        }
    }

    public void OnMenuClose()
    {
        if (menuButton != null)
        {
            menuButton.SetActive(true);
            menuPanel.SetActive(false);
            backgroundOverlay.SetActive(false);
        }
    }
    
    void Start()
    {
        ResetPrompts();
    }

    public void GeneratePrompt()
    {
        if (availablePrompts.Count == 0)
        {
            promptText.text = "You have viewed all prompts available";
            if (promptButton != null) promptButton.SetActive(false);
            return;
        }

        int randomIndex = Random.Range(0, availablePrompts.Count);
        string chosenPrompt = availablePrompts[randomIndex];

        promptText.text = chosenPrompt;

        availablePrompts.RemoveAt(randomIndex);
    }

    private void ResetPrompts()
    {
        availablePrompts = new List<string>(promptList);
        if (promptButton != null) promptButton.SetActive(true);
    }

    public void ShowDebriefPopup()
    {
        if (debriefPopup != null) debriefPopup.SetActive(true);
        backgroundOverlay.SetActive(true);
        menuButton.GetComponent<Button>().interactable = false;
        endButton.SetActive(false);
        menuPanel.SetActive(false);
    }
    
    public void ReturnToLoader()
    {
        SceneManager.LoadScene("UserHome");
    }
}