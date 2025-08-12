/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, October 2023
*/

using AvatarSDK.MetaPerson.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;

namespace AvatarSDK.MetaPerson.MobileIntegrationSample
{
    [System.Serializable]
    public class AvatarProcessRequest
    {
        public string url;
    }

    [System.Serializable]
    public class AvatarProcessResponse
    {
        public string newUrl;
    }

    public class MobileUnitySampleHandler : MonoBehaviour
    {
        [Header("MetaPerson Defaults")]
        public AccountCredentials credentials;

        public MetaPersonLoader metaPersonLoader;
        public GameObject uniWebViewGameObject;
        public GameObject importControls;
        public Text progressText;

        [Header("Create Popup")]
        public GameObject createPopup;
        public Button getAvatarButton;
        public Button proceedButton;
        public GameObject overlayBackground;

        [Header("Start Popup")]
        public GameObject startPopup;
        public Button startButton;
        public TMP_InputField shareCodeInputField;

        private void Start()
        {
            if (credentials.IsEmpty())
            {
                progressText.text = "Error: Account credentials not provided";
                getAvatarButton.interactable = false;
            }

            if (createPopup != null)
            {
                createPopup.SetActive(false);
            }
            if (overlayBackground != null)
            {
                overlayBackground.SetActive(false);
            }
        }

        public void OnCreateAvatarButtonClick()
        {
            if (createPopup != null && overlayBackground != null)
            {
                overlayBackground.SetActive(true);
                createPopup.SetActive(true);
            }
        }

        public void OnProceedButtonClick()
        {
            if (createPopup != null && overlayBackground != null)
            {
                overlayBackground.SetActive(false);
                createPopup.SetActive(false);
            }

            UniWebView uniWebView = uniWebViewGameObject.GetComponent<UniWebView>();
            if (uniWebView == null)
            {
                uniWebView = uniWebViewGameObject.AddComponent<UniWebView>();
                uniWebView.EmbeddedToolbar.Hide();
                uniWebView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            }

            uniWebView.OnPageFinished += OnPageFinished;
            uniWebView.OnMessageReceived += OnMessageReceived;
            uniWebView.Load("https://mobile.metaperson.avatarsdk.com/generator");
            uniWebView.Show();
        }

        public void OnStartButtonClick()
        {
            shareCodeInputField.text = "";
            
            if (startPopup != null && overlayBackground != null)
            {
                overlayBackground.SetActive(true);
                startPopup.SetActive(true);
            }
        }

        public void OnExitButtonClick()
        {
            if (createPopup != null && overlayBackground != null)
            {
                overlayBackground.SetActive(false);
                createPopup.SetActive(false);
            }

            if (startPopup != null && overlayBackground != null)
            {
                overlayBackground.SetActive(false);
                startPopup.SetActive(false);
            }
        }

        public async void OnLoadAvatarWithShareCode()
        {
            if (DBManager.Instance == null)
            {
                Debug.LogError("[DBManager] not available");
                return;
            }

            string shareCode = shareCodeInputField.text.ToUpper().Trim();

            if (string.IsNullOrWhiteSpace(shareCode))
            {
                Debug.LogWarning("missing share code input");
                return;
            }

            Debug.Log($"[DB] searching for avatar with: {shareCode}...");
            progressText.text = "Status: Finding avatar...";

            var avatarData = await DBManager.Instance.GetAvatarByShareCode(shareCode);

            if (avatarData != null)
            {
                // --- PROGRESS CHECK ---
                DateTime today = DateTime.UtcNow;
                DateTime lastLogged = DateTime.Parse(avatarData.last_logged, null, System.Globalization.DateTimeStyles.RoundtripKind);

                if (today.Date > lastLogged.Date)
                {
                    Debug.Log("[Login] new day detected, incrementing days_completed");
                    int newDaysCompleted = avatarData.days_completed + 1;

                    // db update
                    await DBManager.Instance.UpdateAvatarProgress(shareCode, newDaysCompleted, today);

                    avatarData.days_completed = newDaysCompleted;
                }
                else
                {
                    Debug.Log("[Login] no change to days_completed");
                }

                AvatarManager.Instance.SetCurrentAvatar(avatarData.url, avatarData.name, avatarData.days_completed);
                UnityEngine.SceneManagement.SceneManager.LoadScene("AvatarLoader");
            }
            else
            {
                Debug.LogError($"[DB] no avatar found with: {shareCode}");
                progressText.text = "Error: Invalid share code";
            }
        }

        private void OnPageFinished(UniWebView webView, int statusCode, string url)
        {
            string javaScriptCode = @"
                    {
                        function sendConfigurationParams() {
                            console.log('sendConfigurationParams - called');

                            const CLIENT_ID = '" + credentials.clientId + @"';
                            const CLIENT_SECRET = '" + credentials.clientSecret + @"';

                            let authenticationMessage = {
                                'eventName': 'authenticate',
                                'clientId': CLIENT_ID,
                                'clientSecret': CLIENT_SECRET
                            };
                            window.postMessage(authenticationMessage, '*');

                            let exportParametersMessage = {
                                'eventName': 'set_export_parameters',
                                'format': 'glb',
                                'lod': 2,
                                'textureProfile': '1K.jpg'
                            };
                            window.postMessage(exportParametersMessage, '*');

                            let uiParametersMessage = {
                                'eventName': 'set_ui_parameters',
                                'isExportButtonVisible' : true,
                                'isLoginButtonVisible': true
                            };
                            window.postMessage(uiParametersMessage, '*');
                        }

                        function onWindowMessage(evt) {
                            if (evt.type === 'message') {
                                if (evt.data?.source === 'metaperson_creator') {
                                    let data = evt.data;
                                    let evtName = data?.eventName;
                                    if (evtName === 'unity_loaded' ||
                                        evtName === 'mobile_loaded') {
                                        console.log('got mobile_loaded event');
                                        sendConfigurationParams();
                                    } else if (evtName === 'model_exported') {
                                        console.log('got model_exported event');
                                        const params = new URLSearchParams();
                                        params.append('url', data.url);
                                        params.append('gender', data.gender);
                                        params.append('avatarCode', data.avatarCode);
                                        window.location.href = 'uniwebview://model_exported?' + params.toString();
                                    }
                                }
                            }
                        }
                        window.addEventListener('message', onWindowMessage);

                        sendConfigurationParams();
                    }
                ";

            webView.AddJavaScript(javaScriptCode, payload => Debug.LogWarningFormat("JS Execution: {0}", payload.resultCode));
        }

        private void OnMessageReceived(UniWebView webView, UniWebViewMessage message)
        {
            if (message.Path == "model_exported")
            {
                string originalUrl = message.Args["url"];
                Debug.Log("[Server] original url: " + originalUrl);

                webView.Hide();
                getAvatarButton.interactable = false;
                progressText.text = "Status: Modifying Avatar using Blender...";

                // start new server-side processing
                _ = ProcessAvatarOnServer(originalUrl);
            }
        }

        private async Task ProcessAvatarOnServer(string originalUrl)
        {
            string serverApiUrl = "http://172.20.10.3:5000/process-avatar";
            
            var requestData = new AvatarProcessRequest { url = originalUrl };
            string jsonBody = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest www = new UnityWebRequest(serverApiUrl, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                try
                {
                    await www.SendWebRequest();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error: [Server] " + e.Message);
                    progressText.text = "Status: Failed to modify Avatar";
                    getAvatarButton.interactable = true;
                    return; // exit method on failure
                }
            
                string responseJson = www.downloadHandler.text;
                var responseData = JsonUtility.FromJson<AvatarProcessResponse>(responseJson);
                string modifiedUrl = responseData.newUrl;

                Debug.Log("[Server] modified url: " + modifiedUrl);
                progressText.text = "Status: Loading modified Avatar...";

                bool isLoaded = await metaPersonLoader.LoadModelAsync(modifiedUrl, p => progressText.text = string.Format("DOWNLOADING: {0}%", (int)(p * 100)));
                
                if (isLoaded)
                {
                    progressText.text = string.Empty;
                    importControls.SetActive(false);
                    AvatarManager.Instance.SetCurrentAvatar(modifiedUrl, "New Avatar");
                    SceneManager.LoadScene("AvatarDisplay");
                }
                else
                {
                    progressText.text = "Error: Failed to load modified Avatar";
                    getAvatarButton.interactable = true;
                }
            }
        }
    }
}