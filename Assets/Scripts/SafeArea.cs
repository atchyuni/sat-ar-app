using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        ApplySafeArea(); // check every frame
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // apply if:
        // 1. safe area has changed
        // 2. valid, non-zero size
        if (safeArea != lastSafeArea && safeArea.width > 0)
        {
            lastSafeArea = safeArea;

            // convert from pixel to normalized anchor space (0 to 1)
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;

            Debug.Log($"[SafeArea] min({anchorMin.x}, {anchorMin.y}), max({anchorMax.x}, {anchorMax.y})");
        }
    }
}