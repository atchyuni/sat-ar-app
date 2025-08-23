using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

// --- GUIDE ENVIRONMENT CHANGE ---
public class LightDetector : MonoBehaviour
{
    private enum ExercisePhase
    {
        SearchingForDarkness,
        WaitingInDarkness,
        SearchingForBrightness,
        Completed
    }

    [Header("Scene References")]
    public ARCameraManager cameraManager;
    public GameObject messagePanel;
    public TextMeshProUGUI progressText;

    [Header("Lighting Thresholds")]
    [Range(0f, 1f)]
    public float darkThreshold = 0.3f;
    [Range(0f, 2f)]
    public float brightThreshold = 0.5f;

    public float darkDuration = 90f; // 90 sec
    private ExercisePhase currentPhase;
    private float timeInDarkness;
    public float CurrentBrightness { get; private set; }

    void Start()
    {
        currentPhase = ExercisePhase.SearchingForDarkness;
        UpdateInstructions("Please find a dimly lit room");
        timeInDarkness = 0f;
    }

    void OnEnable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    void OnDisable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }
    
    void Update()
    {
        if (currentPhase == ExercisePhase.WaitingInDarkness)
        {
            timeInDarkness += Time.deltaTime;
            
            if (messagePanel != null && messagePanel.activeSelf)
            {
                messagePanel.SetActive(false);
            }

            if (timeInDarkness >= darkDuration)
            {
                currentPhase = ExercisePhase.SearchingForBrightness;
                UpdateInstructions("Please move to a brighter area");
            }
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!eventArgs.lightEstimation.averageBrightness.HasValue)
        {
            return; // no light estimation data available
        }

        CurrentBrightness = eventArgs.lightEstimation.averageBrightness.Value;

        switch (currentPhase)
        {
            case ExercisePhase.SearchingForDarkness:
                if (CurrentBrightness < darkThreshold)
                {
                    // found dark area, start timer phase
                    currentPhase = ExercisePhase.WaitingInDarkness;
                    timeInDarkness = 0f; // reset timer
                }
                break;

            case ExercisePhase.WaitingInDarkness:
                 // if user moves out of dark area, reset timer & phase
                if (CurrentBrightness >= darkThreshold)
                {
                    currentPhase = ExercisePhase.SearchingForDarkness;
                    timeInDarkness = 0f;
                    UpdateInstructions("Please move back to dim lighting");
                }
                break;

            case ExercisePhase.SearchingForBrightness:
                if (CurrentBrightness > brightThreshold)
                {
                    // found bright area, exercise complete
                    currentPhase = ExercisePhase.Completed;
                    if (messagePanel != null) messagePanel.SetActive(false);
                }
                break;
            
            case ExercisePhase.Completed:
                break;
        }
    }

    private void UpdateInstructions(string message)
    {
        if (messagePanel != null && progressText != null)
        {
            messagePanel.SetActive(true);
            progressText.text = message;
        }
    }
}
