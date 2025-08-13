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
    [SerializeField] private GameObject debriefPopup;
    [SerializeField] private GameObject backgroundOverlay;

    void Awake()
    {
        if (instructionBox != null) instructionBox.SetActive(true);
    }

    public void HideInstructions()
    {
        if (instructionBox != null)
        {
            instructionBox.SetActive(false);
        }
    }

    public void ShowDebriefPopup()
    {
        if (debriefPopup != null && backgroundOverlay != null)
        {
            debriefPopup.SetActive(true);
            backgroundOverlay.SetActive(true);
        }
    }
    
    public void ReturnToLoader()
    {
        SceneManager.LoadScene("AvatarLoader");
    }
}