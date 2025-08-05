// adapted from sat-master
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacialSwitcher : MonoBehaviour {
    
    private GameObject avatar;
    private SkinnedMeshRenderer skin;
    private Coroutine transition;

    [SerializeField] private float transitionSpeed = 5.0f;

    [Header("Expression Assets")]
    [SerializeField] private FacialExpression idle;
    [SerializeField] private FacialExpression happy;
    [SerializeField] private FacialExpression sad;
    
    public GameObject Avatar {
        private get => avatar;
        set {
            avatar = value;
            if (avatar != null) {
                var headTransform = avatar.transform.Find("AvatarHead");
                if (headTransform != null) {
                    skin = headTransform.gameObject.GetComponent<SkinnedMeshRenderer>();
                } else {
                    Debug.LogError("unable to find head mesh on avatar");
                }
            }
        }
    }

    private Coroutine Transition {
        get => transition;
        set {
            if (transition != null) {
                StopCoroutine(transition);
            }
            transition = value;
        }
    }
    
    public void ResetFace() {
        if (!skin) return;
        
        int shapeCount = skin.sharedMesh.blendShapeCount;
        for (int i = 0; i < shapeCount; i++) {
            skin.SetBlendShapeWeight(i, 0f);
        }
    }

    public void SetIdle() {
        if (!skin) return;
        Transition = StartCoroutine(TransitionToNeutral());
    }
    
    public void SetHappy() {
        if (!skin) return;
        Transition = StartCoroutine(TransitionToNeutralThenExpression(happy));
    }

    public void SetSad() {
        if (!skin) return;
        Transition = StartCoroutine(TransitionToNeutralThenExpression(sad));
    }

    #region Internal Coroutines

    private void GetCurrentWeights(Dictionary<int, float> weightsCache) {
        if (!skin) return;
        weightsCache.Clear();
        for (int i = 0; i < skin.sharedMesh.blendShapeCount; i++)
        {
            weightsCache[i] = skin.GetBlendShapeWeight(i);
        }
    }
    
    private IEnumerator TransitionToNeutral() {
        Dictionary<int, float> startWeights = new Dictionary<int, float>();
        GetCurrentWeights(startWeights);
        
        float progress = 0.0f;
        while (progress < 1.0f) {
            progress += Time.deltaTime * transitionSpeed;
            foreach (KeyValuePair<int, float> entry in startWeights) {
                skin.SetBlendShapeWeight(entry.Key, Mathf.Lerp(entry.Value, 0, progress));
            }
            yield return null;
        }

        ResetFace(); 
    }

    private IEnumerator TransitionExpression(FacialExpression exp) {
        Dictionary<int, float> startWeights = new Dictionary<int, float>();
        GetCurrentWeights(startWeights);
        
        float progress = 0.0f;
        
        while (progress < 1.0f) {
            progress += Time.deltaTime * transitionSpeed;
            foreach (BlendShapeWeight targetWeight in exp.BlendShapeWeights) {
                float startWeight = startWeights.ContainsKey(targetWeight.index) ? startWeights[targetWeight.index] : 0;
                float currentWeight = Mathf.Lerp(startWeight, targetWeight.weight, progress);
                skin.SetBlendShapeWeight(targetWeight.index, currentWeight);
            }
            yield return null;
        }
    }

    private IEnumerator TransitionToNeutralThenExpression(FacialExpression exp) {
        yield return TransitionToNeutral();
        yield return TransitionExpression(exp);
    }

    #endregion
}