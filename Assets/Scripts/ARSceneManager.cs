using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARSceneManager : MonoBehaviour
{
    [Header("Avatar Prefabs")]
    public GameObject defaultAvatarPrefab; // Assign your default avatar prefab in the Inspector
    public GameObject customAvatarPrefab;  // Assign a base prefab for custom avatars (if you have one)

    [Header("AR Components")]
    public ARRaycastManager arRaycastManager; // Assign AR Raycast Manager from AR Session Origin
    public ARPlaneManager arPlaneManager;     // Assign AR Plane Manager from AR Session Origin

    private GameObject instantiatedAvatar;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        // Ensure AR components are assigned
        if (arRaycastManager == null)
        {
            Debug.LogError("AR Raycast Manager not assigned to ARSceneManager!");
            return;
        }
        if (arPlaneManager == null)
        {
            Debug.LogError("AR Plane Manager not assigned to ARSceneManager!");
            return;
        }

        // Initially disable plane detection until we're ready to place the avatar
        arPlaneManager.planesChanged += OnPlanesChanged; // Subscribe to plane changes
        arPlaneManager.enabled = true; // Enable plane detection
        Debug.Log("AR Plane Manager enabled. Looking for planes...");
    }

    void Update()
    {
        // Only try to place avatar if it hasn't been placed yet
        if (instantiatedAvatar == null)
        {
            TryPlaceAvatar();
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Debugging: Log when planes are added
        foreach (var plane in args.added)
        {
            Debug.Log($"New plane detected: {plane.trackableId}");
        }
    }

    private void TryPlaceAvatar()
    {
        // We want to place the avatar on a detected plane.
        // A simple way is to raycast from the center of the screen.
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hit a plane, get the first hit
            ARRaycastHit hit = hits[0];

            // Load and instantiate the avatar
            LoadAndInstantiateAvatar(hit.pose.position, hit.pose.rotation);

            // Once avatar is placed, you might want to stop plane detection
            // to save resources or prevent new planes from appearing.
            arPlaneManager.enabled = false;
            foreach (var plane in arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(false); // Hide detected planes
            }
            Debug.Log("Avatar placed and plane detection disabled.");
        }
    }

    private void LoadAndInstantiateAvatar(Vector3 position, Quaternion rotation)
    {
        // This is where you'd implement the logic to decide between custom and default.
        // For simplicity, let's assume we use PlayerPrefs to store a flag.
        bool useCustomAvatar = PlayerPrefs.GetInt("UseCustomAvatar", 0) == 1; // 0 for default, 1 for custom

        GameObject avatarToInstantiate = null;

        if (useCustomAvatar && customAvatarPrefab != null)
        {
            Debug.Log("Loading custom avatar...");
            avatarToInstantiate = customAvatarPrefab;
            // Additional logic here to load specific custom avatar data (e.g., colors, textures)
            // and apply it to the customAvatarPrefab instance.
            // This would likely involve loading data saved from the AvatarCreation scene.
            // For example: LoadAvatarCustomizationData(instantiatedAvatar);
        }
        else if (defaultAvatarPrefab != null)
        {
            Debug.Log("Loading default avatar...");
            avatarToInstantiate = defaultAvatarPrefab;
        }
        else
        {
            Debug.LogError("No avatar prefab assigned or found to instantiate!");
            return;
        }

        if (avatarToInstantiate != null)
        {
            instantiatedAvatar = Instantiate(avatarToInstantiate, position, rotation);
            Debug.Log($"Avatar instantiated at {position}");
        }
    }

    // Example of how you might save the preference in the Avatar Creation scene
    // PlayerPrefs.SetInt("UseCustomAvatar", 1); // 1 for custom, 0 for default
    // PlayerPrefs.Save(); // Don't forget to save PlayerPrefs!
}
