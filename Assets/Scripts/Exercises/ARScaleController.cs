using UnityEngine;

public class ARScaleController : MonoBehaviour
{
    public Transform targetTransform;

    [Header("Scaling Settings")]
    [SerializeField] private float pinchScaleSpeed = 0.005f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2.0f;

    // void Update()
    // {
    //     if (targetTransform == null)
    //     {
    //         return;
    //     }

    //     // --- TWO-FINGER PINCH ---
    //     if (Input.touchCount == 2)
    //     {
    //         Touch touchZero = Input.GetTouch(0);
    //         Touch touchOne = Input.GetTouch(1);

    //         Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
    //         Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

    //         // calculate vector magnitude between touches
    //         float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
    //         float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

    //         float difference = currentMagnitude - prevMagnitude;

    //         float newScaleValue = targetTransform.localScale.x + difference * pinchScaleSpeed;
            
    //         // clamp new scale to min/max values
    //         newScaleValue = Mathf.Clamp(newScaleValue, minScale, maxScale);

    //         // apply new scale uniformly
    //         targetTransform.localScale = new Vector3(newScaleValue, newScaleValue, newScaleValue);
    //     }
    // }
}