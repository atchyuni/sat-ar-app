using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float timeInSeconds = 300f; // 5 min * 60 sec

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image startStopButton;
    [SerializeField] private Sprite playIcon;
    [SerializeField] private Sprite pauseIcon;

    private float currentTime;
    private bool isRunning = false;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        if (isRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                Debug.Log("[Timer] finished");
                // reset to play icon
                if (startStopButton != null && playIcon != null)
                {
                    startStopButton.sprite = playIcon;
                }
            }
            
            UpdateTimerText();
        }
    }

    // --- TIMER RUNNING STATE ---
    public void ToggleTimer()
    {
        if (currentTime <= 0) return;

        isRunning = !isRunning;

        if (startStopButton != null && playIcon != null && pauseIcon != null)
        {
            startStopButton.sprite = isRunning ? pauseIcon : playIcon;
        }
    }

    // --- INIT ---
    public void ResetTimer()
    {
        isRunning = false;
        currentTime = timeInSeconds;
        UpdateTimerText();
        
        // reset to play icon
        if (startStopButton != null && playIcon != null)
        {
            startStopButton.sprite = playIcon;
        }
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}