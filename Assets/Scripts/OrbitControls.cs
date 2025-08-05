using UnityEngine;

public class OrbitControls : MonoBehaviour
{
    public Transform target; // target object
    public float distance = 3.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = 1.0f;
    public float distanceMax = 5.0f;
    public float smoothTime = 0.2f;

    public float touchSensitivity = 0.5f;
    public float pinchZoomSpeed = 0.02f;

    private float x = 0.0f;
    private float y = 0.0f;
    private Vector3 rotationVelocity;
    private Vector3 currentRotation;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (target)
        {
            // --- TOUCH CONTROLS ---
            if (Input.touchCount > 0)
            {
                // --- one-finger orbit ---
                if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
                    x += touchDeltaPosition.x * xSpeed * touchSensitivity * 0.02f;
                    y -= touchDeltaPosition.y * ySpeed * touchSensitivity * 0.02f;
                }

                // --- two-finger pinch-to-zoom ---
                if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    distance += deltaMagnitudeDiff * pinchZoomSpeed;
                }
            }
            // --- MOUSE CONTROLS (for editor) ---
            else
            {
                // rotation (turn)
                if (Input.GetMouseButton(0))
                {
                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                }

                // zoom
                distance -= Input.GetAxis("Mouse ScrollWheel") * 2;
            }

            // --- apply all transformations ---
            distance = Mathf.Clamp(distance, distanceMin, distanceMax);
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);

            currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(y, x), ref rotationVelocity, smoothTime);
            Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
            
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }
}