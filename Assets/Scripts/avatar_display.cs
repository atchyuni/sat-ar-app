using UnityEngine;
using AvatarSDK.MetaPerson.Loader;

public class AvatarDisplay : MonoBehaviour
{
    public MetaPersonLoader metaPersonLoader;

    void Start()
    {
        if (metaPersonLoader == null)
        {
            Debug.LogError("metaperson loader not assigned in inspector");
            return;
        }

        // RETRIEVE url from persistent avatar manager
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;

        // check if valid, then load model
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            Debug.Log("loading model from url: " + avatarUrl);
            metaPersonLoader.LoadModel(avatarUrl);
        }
        else
        {
            Debug.LogWarning("no avatar url found");
        }
    }
}