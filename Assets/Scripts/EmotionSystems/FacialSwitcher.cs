// adapted from sat-master
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FacialSwitcher : MonoBehaviour {
    
    private GameObject avatar;
    private SkinnedMeshRenderer skin;
    private Dictionary<int, float> currentWeights = new Dictionary<int, float>();
    private Coroutine transition;

    [SerializeField] private float transitionSpeed = 1.0f;

    [SerializeField] private FacialExpression idle;
    [SerializeField] private FacialExpression happy;
    [SerializeField] private FacialExpression sad;
    
    public GameObject Avatar {
        private get => avatar;
        set {
            avatar = value;
            skin = avatar.transform.Find("AvatarHead").gameObject.GetComponent<SkinnedMeshRenderer>();
        }
    }

    private Coroutine Transition {
        get => transition;
        set {
            if (transition == null) transition = value;
            else {
                StopCoroutine(transition);
                ResetFace();
                transition = value;
            }
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
        Transition = StartCoroutine(TransitionExpression(idle));
    }
    
    public void SetHappy() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(happy));
        
    }

    public void SetSad() {
        if (!skin) return;
        Transition = StartCoroutine(IdleThenTransitionExpression(sad));
    }

    #region Internal

    private void GetCurrentWeights() {
        if (!skin) return;
        
        for (int i = 0; i < skin.sharedMesh.blendShapeCount; i++)
        {
            currentWeights[i] = skin.GetBlendShapeWeight(i);
        }
    }

    private IEnumerator TransitionExpression(FacialExpression exp) {
        
        GetCurrentWeights();
        
        float progress = 0.0f;
        Dictionary<int, float> targetWeights = new Dictionary<int, float>(currentWeights);

        foreach (BlendShapeWeight weight in exp.BlendShapeWeights) {
            targetWeights[weight.index] = weight.weight;
        }
        
        while (progress < 1.0f) {
            progress += Time.deltaTime * transitionSpeed;
            foreach (KeyValuePair<int,float> targetWeight in targetWeights) {
                int index = targetWeight.Key;
                float currentWeight = Mathf.Lerp(currentWeights[index], targetWeight.Value, progress);
                skin.SetBlendShapeWeight(index, currentWeight);
            }
            yield return null;
        }
    }

    private IEnumerator IdleThenTransitionExpression(FacialExpression exp) {
        yield return TransitionExpression(idle);
        yield return TransitionExpression(exp);
    }

    #endregion
}