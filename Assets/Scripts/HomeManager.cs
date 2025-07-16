// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using AvatarSDK.MetaPerson.Loader;
// using AvatarSDK.MetaPerson.Creator;

// // 1. launches metaperson creator
// // 2. transitions to display scene
// public class HomeManager : MonoBehaviour
// {
//     public void CreateAvatarButton_Clicked()
//     {
//         var metaPersonCreator = FindObjectOfType<MetaPersonCreator>();
//         if (metaPersonCreator != null)
//         {
//             // subscribe to creating event
//             metaPersonCreator.AvatarCreated += OnAvatarCreated;
//             metaPersonCreator.CreateAvatar();
//         }
//         else
//         {
//             Debug.LogError("MetaPersonCreator not found in scene");
//         }
//     }

//     private void OnAvatarCreated(string avatarUrl)
//     {
//         Debug.Log($"avatar created with URL: {avatarUrl}");

//         // unsubscribe to creating event
//         var metaPersonCreator = FindObjectOfType<MetaPersonCreator>();
//         if (metaPersonCreator != null)
//         {
//             metaPersonCreator.AvatarCreated -= OnAvatarCreated;
//         }

//         // store URL for next scene
//         PlayerPrefs.SetString("AvatarURL", avatarUrl);
//         PlayerPrefs.Save();

//         SceneManager.LoadScene("AvatarDisplayScene");
//     }

//     public void StartButton_Clicked()
//     {
//         // TODO: logic to prompt for share code
//         SceneManager.LoadScene("AvatarDisplayScene");
//     }
// }
