using UnityEngine;
using AvatarSDK.MetaPerson.Loader;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;

public class AvatarDisplay : MonoBehaviour
{

    public MetaPersonLoader avatarLoader;

    [Header("Expression Systems")]
    public RuntimeAnimatorController controller;
    [SerializeField] private AnimationManager animationManager;
    [SerializeField] private FacialSwitcher facialSwitcher;

    [Header("UI References")]
    [SerializeField] private Toggle happyFace;
    [SerializeField] private Toggle sadFace;
    [SerializeField] private Toggle happyBody;
    [SerializeField] private Toggle sadBody;

    private bool initialised = false;


    async void Start()
    {
        if (avatarLoader == null)
        {
            return;
        }

        string avatar_url = AvatarManager.Instance.CurrentAvatarUrl;

        // --- LOAD MODEL ---
        if (!string.IsNullOrEmpty(avatar_url))
        {
            Debug.Log("[AvatarDisplay] loading model from: " + avatar_url);

            bool loaded = await avatarLoader.LoadModelAsync(avatar_url);

            if (loaded && avatarLoader.transform.childCount > 0)
            {
                GameObject avatar = avatarLoader.transform.GetChild(0).gameObject;
                avatar.transform.rotation = Quaternion.Euler(0, 180, 0);

                // --- EMOTION SYSTEMS ASSIGNMENT ---
                Animator animator = avatar.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = controller;
                    animationManager.animationSwitcher.Animator = animator;
                    animationManager.animationSwitcher.SetupOverrideController();
                }
                else
                {
                    Debug.LogError("[AvatarDisplay] animator not found");
                }

                facialSwitcher.Avatar = avatar;
                animator.gameObject.AddComponent<AnimEventReceiver>().ans = animationManager.animationSwitcher;

                // default states
                SetIdleFace();
                SetIdleBody();
                initialised = true;

                Debug.Log("[AvatarDisplay] systems initialised successfully");
            }
        }
        else
        {
            Debug.LogWarning("[AvatarManager] missing avatar url");
        }
    }
    
    // --- UI METHODS ---
    public void HappyFaceToggle(bool on) { if (on && initialised) facialSwitcher.SetHappy(); }
    public void SadFaceToggle(bool on) { if (on && initialised) facialSwitcher.SetSad(); }
    public void SetIdleFace() {
        if (!initialised) return;
        if(happyFace) happyFace.isOn = false;
        if(sadFace) sadFace.isOn = false;
        facialSwitcher.SetIdle();
    }

    public void HappyBodyToggle(bool on) { if (on && initialised) animationManager.Happy(); }
    public void SadBodyToggle(bool on) { if (on && initialised) animationManager.Cry(); }
    public void SetIdleBody() {
        if (!initialised) return;
        if(happyBody) happyBody.isOn = false;
        if(sadBody) sadBody.isOn = false;
        animationManager.SetIdle();
    }
}