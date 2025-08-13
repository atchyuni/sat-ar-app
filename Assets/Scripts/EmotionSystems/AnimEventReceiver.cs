// adapted from sat-master
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventReceiver : MonoBehaviour {
    
    public AnimationSwitcher ans;
    
    public void AnimationEnd() {
        ans.AnimationEnd();
    }
}
