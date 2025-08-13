// adapted from sat-master
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour {
    
    [SerializeField] public AnimationSwitcher animationSwitcher;

    [Header("Animation Clips")]
    public AnimationClip idle;
    public AnimationClip happy;
    public AnimationClip cry;
    
    public void SetIdle() {
        if (animationSwitcher != null) animationSwitcher.PlayAnimationClipLooped(idle);
    }

    public void Happy() {
        if (animationSwitcher != null) animationSwitcher.PlayAnimationClipLooped(happy);
    }
    
    public void Cry() {
        if (animationSwitcher != null) animationSwitcher.PlayAnimationClipLooped(cry);
    }
}
