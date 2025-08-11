using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private Slider progressBarSlider;
    [SerializeField] private TMP_Text progressText;

    private const int MAX_DAYS = 56;

    public void UpdateProgress(int currentDays)
    {
        if (progressBarSlider == null || progressText == null)
        {
            Debug.LogError("[ProcessBarUI] references not set");
            return;
        }

        // update text & slider fill
        progressText.text = $"{currentDays}/{MAX_DAYS}";
        progressBarSlider.value = (float)currentDays / MAX_DAYS;
    }
}