using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BlendShapeWeight
{
    public int index;
    [Range(0, 100)] public float weight;
}

[CreateAssetMenu(fileName = "New Facial Expression", menuName = "Avatar SDK/Facial Expression")]
public class FacialExpression : ScriptableObject
{
    public List<BlendShapeWeight> BlendShapeWeights;
}