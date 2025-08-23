using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(ARRaycastManager))]
public class ARVaseHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject instructionBox;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject debriefPopup;
    [SerializeField] private GameObject backgroundOverlay;
    [SerializeField] private GameObject endButton;

    void Awake()
    {
        if (instructionBox != null) instructionBox.SetActive(true);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);
        if (menuButton != null) menuButton.SetActive(true);
        menuButton.GetComponent<Button>().interactable = false;
        if (menuPanel != null) menuPanel.SetActive(false);
        if (endButton != null) endButton.SetActive(true);
        endButton.GetComponent<Button>().interactable = false;
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