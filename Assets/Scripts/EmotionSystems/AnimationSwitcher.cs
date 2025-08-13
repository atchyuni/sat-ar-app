// adapted from sat-master
using System.Collections;
using System.Linq;
using UnityEngine;

public class AnimationSwitcher : MonoBehaviour
{
    public AnimatorOverrideController OverrideController { get; private set; }
    public Animator Animator;
    public AnimationClip IdleAnimation;

    private readonly AlternatingStates _states = new();

    private class AlternatingStates
    {
        private readonly string[] _keyNames = { "__Placeholder_1", "__Placeholder_2" };
        public int StateIndex { get; private set; } = 1;

        public void Toggle()
        {
            StateIndex = 1 - StateIndex;
        }

        public string CurrentKeyForOverride
        {
            get => _keyNames[StateIndex];
        }
    }

    // void Start()
    // {
    //     OverrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
    //     Animator.runtimeAnimatorController = OverrideController;
    // }
    
    public void SetupOverrideController()
    {
        if (Animator != null && Animator.runtimeAnimatorController != null)
        {
            OverrideController = new AnimatorOverrideController(Animator.runtimeAnimatorController);
            Animator.runtimeAnimatorController = OverrideController;
            Debug.Log("[SetupOverrideController] successful");
        }
        else
        {
            Debug.LogError("[AnimationSwitcher] null animator or runtime controller");
        }
    }

    public void PlayAnimationClip(AnimationClip animationClip)
    {
        if (OverrideController != null)
        {
            AddEndEvent(animationClip);
            OverrideController[_states.CurrentKeyForOverride] = animationClip;
            Animator.SetInteger("AnimationIndex", _states.StateIndex);
            _states.Toggle();
        }
    }

    public void PlayAnimationClipLooped(AnimationClip animationClip) {
        if (OverrideController != null) {
            animationClip.wrapMode = WrapMode.Loop;
            OverrideController[_states.CurrentKeyForOverride] = animationClip;
            Animator.SetInteger("AnimationIndex", _states.StateIndex);
            _states.Toggle();
        }
    }

    public void SwitchAvatar(Avatar avatar) {
        Animator.avatar = avatar;
        //
        //  Don't you dare changing this, my dear pedant future-self or a colleague.
        //  It just werks.
        //
        StartCoroutine(RebindAnimatorNextFrame());
    }

    private IEnumerator RebindAnimatorNextFrame()
    {
        yield return null;  // Wait for the next frame
        Animator.Rebind();
    }


    private void AddEndEvent(AnimationClip clip) {
        
        bool exists = clip.events.Any(animationEvent => animationEvent.functionName == "AnimationEnd");

        if (exists) return;
        
        Debug.Log("Adding new event to clip: " + clip.name);
        AnimationEvent endEvent = new AnimationEvent {
            time = clip.length - 0.03f,
            functionName = "AnimationEnd"
        };
        clip.AddEvent(endEvent);
    }

    public void AnimationEnd() {
        PlayAnimationClipLooped(IdleAnimation);
    }
}
