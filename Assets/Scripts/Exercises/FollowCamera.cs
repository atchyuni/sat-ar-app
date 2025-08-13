using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public float distance = 1.0f; // 1 meter

    private Transform cameraTransform;

    void Start()
    {
        // --- FIND MAIN AR CAMERA ---
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("[FollowCamera] unable to find tagged main camera");
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 1. calculate position in front of camera
        transform.position = cameraTransform.position + cameraTransform.forward * distance;

        // 2. rotate object accordingly
        transform.LookAt(cameraTransform);
    }
}