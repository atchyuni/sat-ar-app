using UnityEngine;
using UnityEngine.UI;

public class AvatarCustomiser : MonoBehaviour
{
    [Header("System References")]
    public FacialSwitcher facialSwitcher;
    public AnimationManager animationManager;

    [Header("UI References")]
    public ToggleGroup facialExpressionGroup;
    public ToggleGroup bodyAnimationGroup;
    public Toggle happyFaceToggle;
    public Toggle sadFaceToggle;
    public Toggle happyBodyToggle;
    public Toggle sadBodyToggle;

    private GameObject currentAvatar;
    private bool isInitialized = false;

    public void Initialize(GameObject avatarObject)
    {
        currentAvatar = avatarObject;
        if (currentAvatar == null)
        {
            Debug.LogError("null avatar object");
            return;
        }

        // connect systems to loaded avatar
        facialSwitcher.Avatar = currentAvatar;
        animationManager.BindAnimationSwitcher(currentAvatar);

        // set default idle state
        SetIdleFace();
        SetIdleBody();
        isInitialized = true;

        // Debug.Log("---------- AVATAR HIERARCHY ----------");
        // PrintHierarchy(avatarObject.transform, "");
        // Debug.Log("------------------------------------");
    }

    // private void PrintHierarchy(Transform t, string indent)
    // {
    //     Debug.Log(indent + t.name);
    //     foreach (Transform child in t)
    //     {
    //         PrintHierarchy(child, indent + "  ");
    //     }
    // }

    #region Facial Expression Methods
    public void OnHappyFaceToggle(bool isOn)
    {
        if (isOn && isInitialized) facialSwitcher.SetHappy();
    }

    public void OnSadFaceToggle(bool isOn)
    {
        if (isOn && isInitialized) facialSwitcher.SetSad();
    }

    public void SetIdleFace()
    {
        if (!isInitialized) return;
        if (facialExpressionGroup != null)
        {
            facialExpressionGroup.SetAllTogglesOff();
        }
        
        facialSwitcher.SetIdle();
    }
    #endregion

    #region Body Animation Methods
    public void OnHappyBodyToggle(bool isOn)
    {
        if (isOn && isInitialized) animationManager.Happy();
    }

    public void OnSadBodyToggle(bool isOn)
    {
        if (isOn && isInitialized) animationManager.Cry();
    }

    public void SetIdleBody()
    {
        if (!isInitialized) return;
        if (bodyAnimationGroup != null)
        {
            bodyAnimationGroup.SetAllTogglesOff();
        }

        animationManager.SetIdle();
    }
    #endregion
}