using UnityEngine;
using UnityEngine.UI;

public class AvatarCustomiser : MonoBehaviour
{
    [Header("System References")]
    public FacialSwitcher facialSwitcher;
    public AnimationManager animationManager;

    [Header("UI Toggles")]
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
    }

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
        happyFaceToggle.isOn = false;
        sadFaceToggle.isOn = false;
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
        happyBodyToggle.isOn = false;
        sadBodyToggle.isOn = false;
        animationManager.SetIdle();
    }
    #endregion
}