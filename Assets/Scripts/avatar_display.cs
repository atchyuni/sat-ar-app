using UnityEngine;
using AvatarSDK.MetaPerson.Loader;
using System.Threading.Tasks;

public class AvatarDisplay : MonoBehaviour
{
    public MetaPersonLoader metaPersonLoader;
    public OrbitControls cameraControls;
    public Vector3 avatarWorldPosition = new Vector3(-0.6f, 0, 0);

    async void Start()
    {
        if (metaPersonLoader == null || cameraControls == null)
        {
            Debug.LogError("metaperson loader or camera controls not assigned in inspector");
            return;
        }

        // FETCH url from persistent avatar manager
        string avatarUrl = AvatarManager.Instance.CurrentAvatarUrl;

        // check if valid, then load model
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            Debug.Log("loading model from url: " + avatarUrl);

            bool isLoaded = await metaPersonLoader.LoadModelAsync(avatarUrl);

            if (isLoaded && metaPersonLoader.transform.childCount > 0)
            {
                // -- 1. FETCH newly loaded avatar, CREATE pivot --
                GameObject avatarObject = metaPersonLoader.transform.GetChild(0).gameObject;
                GameObject avatarPivot = new GameObject("AvatarPivot");

                avatarPivot.transform.position = avatarWorldPosition;

                avatarObject.transform.SetParent(avatarPivot.transform); // set pivot

                // -- 2. move model down relative to new pivot --
                avatarObject.transform.localPosition = new Vector3(0, -0.9f, 0);

                // -- 3. rotate new pivot (forward face avatar) --
                avatarPivot.transform.rotation = Quaternion.Euler(0, 180, 0);

                // -- 4. set camera controls --
                cameraControls.target = avatarPivot.transform;
            }
        }
        else
        {
            Debug.LogWarning("no avatar url found");
        }
    }
}