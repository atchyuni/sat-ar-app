using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ARRaycastManager))]
public class ARVaseHandler : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject instructionBox;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject debriefPopup;
    [SerializeField] private GameObject backgroundOverlay;

    void Awake()
    {
        if (instructionBox != null) instructionBox.SetActive(true);
        if (menuButton != null) menuButton.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void HideInstructions()
    {
        if (instructionBox != null)
        {
            instructionBox.SetActive(false);
            menuButton.SetActive(true);
        }
    }

    public void OnMenuClick()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            menuButton.SetActive(false);
        }
    }

    public void OnMenuClose()
    {
        if (menuButton != null)
        {
            menuButton.SetActive(true);
            menuPanel.SetActive(false);
        }
    }

    public void ShowDebriefPopup()
    {
        if (debriefPopup != null && backgroundOverlay != null)
        {
            if (menuPanel != null) menuPanel.SetActive(false);
            if (menuButton != null) menuButton.SetActive(false);
            
            debriefPopup.SetActive(true);
            backgroundOverlay.SetActive(true);
        }
    }
    
    public void ReturnToLoader()
    {
        SceneManager.LoadScene("UserHome");
    }
}