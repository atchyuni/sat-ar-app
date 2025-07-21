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

        // retrieve URL from persistent AvatarManager
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;

        // check if URL valid, then load model
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            Debug.Log("found avatar URL, loading model: " + avatarUrl);
            metaPersonLoader.LoadModel(avatarUrl);
        }
        else
        {
            Debug.LogWarning("no avatar URL found");
        }
    }
}