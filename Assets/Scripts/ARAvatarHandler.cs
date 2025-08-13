using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using AvatarSDK.MetaPerson.Loader;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(ARRaycastManager))]
public class ARAvatarHandler : MonoBehaviour
{
    [Header("Avatar Components")]
    [SerializeField] private MetaPersonLoader metaPersonLoader;
    [SerializeField] private GameObject placementIndicator;

    [Header("UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject instructionBox;
    [SerializeField] private GameObject emotionButton;
    [SerializeField] private GameObject emotionControls;
    [SerializeField] private GameObject debriefPopup;
    [SerializeField] private GameObject backgroundOverlay;

    [SerializeField] private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private GameObject placedAvatarInstance;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        if (instructionBox != null) instructionBox.SetActive(true);
        if (placementIndicator != null) placementIndicator.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    void Update()
    {
        if (instructionBox != null && instructionBox.activeInHierarchy)
        {
            return;
        }

        if (placedAvatarInstance == null)
        {
            UpdatePlacementIndicator();
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // --- UI TAP CHECK ---
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                Debug.Log("[ARAvatarHandler] ignoring ui tap for ar placement");
                return;
            }

            if (arRaycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;

                if (placedAvatarInstance == null)
                {
                    PlaceAvatarAsync(hitPose);
                }
                else
                {
                    // --- REPOSITIONING (RAYCAST) ---
                    // 1. set new position
                    placedAvatarInstance.transform.position = hitPose.position;

                    // 2. rotate to face camera (horizontally)
                    Vector3 cameraPosition = Camera.main.transform.position;
                    Vector3 directionToCamera = cameraPosition - placedAvatarInstance.transform.position;
                    directionToCamera.y = 0;
                    placedAvatarInstance.transform.rotation = Quaternion.LookRotation(directionToCamera);
                }
            }
        }
    }

    public void HideInstructions()
    {
        if (instructionBox != null)
        {
            instructionBox.SetActive(false);
        }
    }

    public void OnEmotionClick()
    {
        if (emotionControls != null)
        {
            emotionControls.SetActive(true);
            emotionButton.SetActive(false);
        }
    }

    private async void PlaceAvatarAsync(Pose placementPose)
    {
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;
        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("[AvatarManager] missing url");
            return;
        }

        placementIndicator.SetActive(false);

        try
        {
            if (loadingPanel != null) loadingPanel.SetActive(true);

            bool isLoaded = await metaPersonLoader.LoadModelAsync(avatarUrl);
            if (isLoaded)
            {
                GameObject originalAvatar = metaPersonLoader.transform.GetChild(0).gameObject;
                placedAvatarInstance = Instantiate(originalAvatar);
                originalAvatar.SetActive(false);

                placedAvatarInstance.transform.position = placementPose.position;
                placedAvatarInstance.transform.rotation = placementPose.rotation;
                placedAvatarInstance.transform.Rotate(0, 180, 0);

                SetPlaneVisualsActive(false);
            }
        }
        finally
        {
            if (loadingPanel != null) loadingPanel.SetActive(false);
        }
    }

    private void UpdatePlacementIndicator()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
        else
        {
            if (placementIndicator != null) placementIndicator.SetActive(false);
        }
    }

    // --- TOGGLE PLANE VISUALS ---
    private void SetPlaneVisualsActive(bool isActive)
    {
        if (arPlaneManager == null) return;

        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(isActive);
        }
    }

    public void ShowDebriefPopup()
    {
        if (debriefPopup != null && backgroundOverlay != null)
        {
            if (emotionControls != null) emotionControls.SetActive(false);
            if (emotionButton != null) emotionButton.SetActive(false);

            debriefPopup.SetActive(true);
            backgroundOverlay.SetActive(true);
        }
    }
    
    public void ReturnToLoader()
    {
        SceneManager.LoadScene("AvatarLoader");
    }
}