using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class ChecklistManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject backgroundOverlay;
    [SerializeField] private GameObject checklistPanel;
    [SerializeField] private Toggle sleepToggle;
    [SerializeField] private Toggle dietToggle;
    [SerializeField] private Toggle exerciseToggle;
    [SerializeField] private GameObject redirectMessage;

    private Action onSuccessCallback; // to ensure single check per login

    // private void Start()
    // {
    //     if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
    //     if (checklistPanel != null) checklistPanel.SetActive(false);
    //     if (redirectMessage != null) redirectMessage.SetActive(false);
    // }

    public void ShowChecklist(Action onSuccess)
    {
        onSuccessCallback = onSuccess;

        // reset ui state each time
        redirectMessage.SetActive(false);
        sleepToggle.isOn = false;
        dietToggle.isOn = false;
        exerciseToggle.isOn = false;

        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);
        if (checklistPanel != null) checklistPanel.SetActive(true);
    }

    public void OnProceedClicked()
    {
        if (sleepToggle.isOn && dietToggle.isOn && exerciseToggle.isOn)
        {
            if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
            if (checklistPanel != null) checklistPanel.SetActive(false);
            onSuccessCallback?.Invoke();
        }
        else
        {
            if (redirectMessage != null) redirectMessage.SetActive(true);
        }
    }
    
    public void OnReturnToTitleClicked()
    {
        SceneManager.LoadScene("Startup");
    }
}