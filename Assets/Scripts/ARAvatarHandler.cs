using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using AvatarSDK.MetaPerson.Loader;

[RequireComponent(typeof(ARRaycastManager))]
public class ARAvatarHandler : MonoBehaviour
{
    [Header("Avatar Components")]
    [SerializeField] private MetaPersonLoader metaPersonLoader;
    [SerializeField] private GameObject placementIndicator;

    private ARRaycastManager arRaycastManager;
    private GameObject placedAvatarInstance;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        if (placementIndicator != null) placementIndicator.SetActive(false);
    }

    void Update()
    {
        UpdatePlacementIndicator();

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (placementIndicator.activeSelf) // place if indicator on valid plane
            {
                if (placedAvatarInstance == null)
                {
                    PlaceAvatarAsync();
                }
                else
                {
                    placedAvatarInstance.transform.position = placementIndicator.transform.position;
                    placedAvatarInstance.transform.rotation = placementIndicator.transform.rotation;
                }
            }
        }
    }

    private async void PlaceAvatarAsync()
    {
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;
        if (string.IsNullOrEmpty(avatarUrl))
        {
            Debug.LogError("[AvatarManager] missing url");
            return;
        }

        // store placement & disable indicator while loading
        Vector3 placementPosition = placementIndicator.transform.position;
        Quaternion placementRotation = placementIndicator.transform.rotation;
        placementIndicator.SetActive(false);
        
        // TODO: "Loading in AR Environment..." UI
        
        bool isLoaded = await metaPersonLoader.LoadModelAsync(avatarUrl);
        if (isLoaded)
        {
            // create independent instance of avatar & position
            GameObject originalAvatar = metaPersonLoader.transform.GetChild(0).gameObject;
            placedAvatarInstance = Instantiate(originalAvatar);
            originalAvatar.SetActive(false);

            placedAvatarInstance.transform.position = placementPosition;
            placedAvatarInstance.transform.rotation = placementRotation;
            placedAvatarInstance.transform.Rotate(0, 180, 0);

            // TODO: hide "Loading..." UI
        }
    }

    private void UpdatePlacementIndicator()
    {
        // shoot ray from screen center
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        
        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // if plane hit, update indicator pose
            var hitPose = hits[0].pose;
            if (placementIndicator != null)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            }
        }
        else
        {
            // else, hide indicator
            if (placementIndicator != null) placementIndicator.SetActive(false);
        }
    }
}