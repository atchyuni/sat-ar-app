// adapted from sat-master
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour {
    
    [SerializeField] private AnimationSwitcher ans;

    public AnimationClip idle;
    public AnimationClip dance;
    public AnimationClip happy;
    public AnimationClip cry;

    public void BindAnimationSwitcher(GameObject avatarObject) {
        ans.SwitchAvatar(avatarObject);
    }
    
    public void SetIdle() {
        ans.PlayAnimationClipLooped(idle);
    }

    public void Dance() {
        ans.PlayAnimationClip(dance);
    }

    public void Happy() {
        ans.PlayAnimationClip(happy);
    }
    
    public void Cry() {
        ans.PlayAnimationClip(cry);
    }
}
