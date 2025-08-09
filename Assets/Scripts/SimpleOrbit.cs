using UnityEngine;

public class SimpleOrbit : MonoBehaviour
{
    public Transform target;

    [Header("Settings")]
    public float distance = 2.5f;
    public float sensitivity = 2.0f;
    public Vector3 targetOffset = Vector3.zero;
    private float x = 0.0f;
    private float y = 0.0f;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // --- INIT ---
        // if x and y are zero, then new target
        // init rotation based on camera current direction
        if (x == 0 && y == 0)
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
        }

        // --- INPUT HANDLING ---
        if (Input.GetMouseButton(0))
        {
            x += Input.GetAxis("Mouse X") * sensitivity;
            y -= Input.GetAxis("Mouse Y") * sensitivity;
            y = Mathf.Clamp(y, -20f, 80f);
        }
        distance -= Input.GetAxis("Mouse ScrollWheel") * 2;
        distance = Mathf.Clamp(distance, 1.0f, 5.0f);

        // --- APPLY TRANSFORMATION ---
        Vector3 orbitPoint = target.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = orbitPoint + (rotation * new Vector3(0.0f, 0.0f, -distance));

        transform.rotation = rotation;
        transform.position = position;
    }
}