// adapted from sat-master
using UnityEngine;

public class AnimationSwitcher : MonoBehaviour
{
    private Animation animationComponent;

    public void SwitchAvatar(GameObject avatar)
    {
        if (avatar == null)
        {
            Debug.LogError("null avatar game object in switch");
            return;
        }

        animationComponent = avatar.GetComponent<Animation>();
        if (animationComponent == null)
        {
            animationComponent = avatar.AddComponent<Animation>();
        }
    }

    public void PlayAnimationClipLooped(AnimationClip clip)
    {
        if (animationComponent == null || clip == null) return;

        animationComponent.AddClip(clip, clip.name);
        
        animationComponent.clip = clip;

        animationComponent.wrapMode = WrapMode.Loop;
        animationComponent.Play(clip.name);
    }

    public void PlayAnimationClip(AnimationClip clip)
    {
        if (animationComponent == null || clip == null) return;
        animationComponent.AddClip(clip, clip.name);
        animationComponent.Play(clip.name);
    }
}