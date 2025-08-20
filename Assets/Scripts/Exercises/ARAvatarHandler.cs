using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using AvatarSDK.MetaPerson.Loader;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(ARRaycastManager))]
public class ARAvatarHandler : MonoBehaviour
{
    [Header("Avatar Components")]
    [SerializeField] private MetaPersonLoader avatarLoader;
    [SerializeField] private GameObject placementIndicator;
    private GameObject placedAvatar;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject instructionBox;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lockButton;
    [SerializeField] private GameObject debriefPopup;
    [SerializeField] private GameObject backgroundOverlay;
    [SerializeField] private Toggle happyFace;
    [SerializeField] private Toggle sadFace;
    [SerializeField] private Toggle happyBody;
    [SerializeField] private Toggle sadBody;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    public Image lockButtonImage;

    [Header("Emotion Controls")]
    [SerializeField] private GameObject emotionButton;
    [SerializeField] private GameObject emotionControls;

    [Header("Expression Systems")]
    public RuntimeAnimatorController controller;
    [SerializeField] private AnimationManager animationManager;
    [SerializeField] private FacialSwitcher facialSwitcher;

    [SerializeField] private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    [SerializeField] private ARScaleController scaleController;
    private bool initialised = false;
    private bool locked = false;

    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        if (instructionBox != null) instructionBox.SetActive(true);
        if (placementIndicator != null) placementIndicator.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (menuButton != null) menuButton.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (lockButton != null) lockButton.SetActive(false);
    }

    void Update()
    {
        if (placedAvatar == null)
        {
            UpdatePlacementIndicator();
        }

        // only allow repositioning if avatar not locked
        if (!locked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // --- UI TAP CHECK ---
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                Debug.Log("[ARHandler] tap not for placement");
                return;
            }

            if (arRaycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;

                if (placedAvatar == null)
                {
                    PlaceAvatarAsync(hitPose);
                }
                else
                {
                    // --- REPOSITIONING (RAYCAST) ---
                    // 1. set new position
                    placedAvatar.transform.position = hitPose.position;

                    // 2. rotate to face camera (horizontally)
                    Vector3 cameraPosition = Camera.main.transform.position;
                    Vector3 directionToCamera = cameraPosition - placedAvatar.transform.position;
                    directionToCamera.y = 0;
                    placedAvatar.transform.rotation = Quaternion.LookRotation(directionToCamera);
                }
            }
        }
    }

    public void ToggleLock()
    {
        locked = !locked;

        SetPlaneVisualsActive(!locked);

        if (lockButtonImage != null)
        {
            if (locked)
            {
                lockButtonImage.sprite = lockedSprite;
            }
            else
            {
                lockButtonImage.sprite = unlockedSprite;
            }
        }
        Debug.Log($"[ARHandler] avatar position locked: {locked}");
    }

    public void HideInstructions()
    {
        if (instructionBox != null)
        {
            instructionBox.SetActive(false);
            menuButton.SetActive(true);
            lockButton.SetActive(true);
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
        string avatar_url = AvatarManager.Instance.CurrentAvatarUrl;
        if (string.IsNullOrEmpty(avatar_url))
        {
            Debug.LogError("[AvatarManager] missing url");
            return;
        }

        placementIndicator.SetActive(false);
        GameObject indicator_to_destroy = placementIndicator;
        placementIndicator = null;

        try
        {
            if (loadingPanel != null) loadingPanel.SetActive(true);

            Debug.Log($"[ARHandler] loading with: {avatar_url}");
            bool loaded = await avatarLoader.LoadModelAsync(avatar_url);
            if (loaded && avatarLoader.transform.childCount > 0)
            {
                GameObject avatar = avatarLoader.transform.GetChild(0).gameObject;
                placedAvatar = Instantiate(avatar);
                placedAvatar.tag = "PlayerAvatar";
                
                avatar.SetActive(false);

                placedAvatar.transform.position = placementPose.position;
                placedAvatar.transform.rotation = placementPose.rotation;
                placedAvatar.transform.Rotate(0, 180, 0);

                if (scaleController != null)
                {
                    scaleController.targetTransform = placedAvatar.transform;
                }

                Animator animator = placedAvatar.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = controller;
                    animationManager.animationSwitcher.Animator = animator;
                    animationManager.animationSwitcher.SetupOverrideController();
                }
                else
                {
                    Debug.LogError("[ARHandler] animator not found on loaded avatar");
                }

                facialSwitcher.Avatar = placedAvatar;
                animator.gameObject.AddComponent<AnimEventReceiver>().ans = animationManager.animationSwitcher;

                SetIdleFace();
                SetIdleBody();
                initialised = true;
                Debug.Log("[ARHandler] systems initialised successfully");

                SetPlaneVisualsActive(false);
            }
        }
        finally
        {
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (indicator_to_destroy != null) Destroy(indicator_to_destroy);
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

    public void ShowDebriefPopup()
    {
        if (debriefPopup != null && backgroundOverlay != null)
        {
            if (emotionControls != null) emotionControls.SetActive(false);
            if (emotionButton != null) emotionButton.SetActive(false);
            if (menuPanel != null) menuPanel.SetActive(false);
            if (menuButton != null) menuButton.SetActive(false);
            if (lockButton != null) lockButton.SetActive(false);

            debriefPopup.SetActive(true);
            backgroundOverlay.SetActive(true);
        }
    }
    
    public void ReturnToLoader()
    {
        SceneManager.LoadScene("UserHome");
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
}