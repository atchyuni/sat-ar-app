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
using GLTFast;
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
        public GameObject uniWebViewObject;
        public GameObject importControls;
        public Text progressText;

        [Header("Create Avatar Popup")]
        public GameObject createPopup;
        public Button getAvatarButton;
        public Button proceedButton;
        public GameObject overlayBackground;

        [Header("Start Popup")]
        public GameObject startPopup;
        public Button startButton;
        public TMP_InputField shareCodeInput;
        
        private void Start()
        {
            CleanupRogues();

            if (credentials.IsEmpty())
            {
                progressText.text = "Error: Contact admin for account credentials";
                getAvatarButton.interactable = false;
            }

            if (createPopup != null) createPopup.SetActive(false);
            if (startPopup != null) startPopup.SetActive(false);
            if (overlayBackground != null) overlayBackground.SetActive(false);
        }

        private void CleanupRogues()
    {
        var avatars = FindObjectsOfType<TimeBudgetPerFrameDeferAgent>(true);

        if (avatars.Length > 0)
        {
            Debug.LogWarning($"[SceneHandler] found {avatars.Length} rogue glTF model(s), destroying");
            foreach (var avatarComponent in avatars)
            {
                Destroy(avatarComponent.gameObject);
            }
        }
    }

        public void OnCreateAvatar()
        {
            if (createPopup != null && overlayBackground != null)
            {
                createPopup.SetActive(true);
                overlayBackground.SetActive(true);
            }
        }

        public void OnProceedClick()
        {
            if (createPopup != null && overlayBackground != null)
            {
                createPopup.SetActive(false);
                overlayBackground.SetActive(false);
            }

            UniWebView uniWebView = uniWebViewObject.GetComponent<UniWebView>();
            if (uniWebView == null)
            {
                uniWebView = uniWebViewObject.AddComponent<UniWebView>();
                uniWebView.EmbeddedToolbar.Hide();
                uniWebView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            }

            uniWebView.OnPageFinished += OnPageFinished;
            uniWebView.OnMessageReceived += OnMessageReceived;
            uniWebView.Load("https://mobile.metaperson.avatarsdk.com/generator");
            uniWebView.Show();
        }

        public void OnStart()
        {
            shareCodeInput.text = "";

            if (startPopup != null && overlayBackground != null)
            {
                startPopup.SetActive(true);
                overlayBackground.SetActive(true);
            }
        }

        public void OnClose()
        {
            overlayBackground.SetActive(false);
            createPopup.SetActive(false);
            startPopup.SetActive(false);
        }

        public async void LoadAvatarWithShareCode()
        {
            if (DBManager.Instance == null)
            {
                Debug.LogError("[DBManager] not available");
                return;
            }

            string shareCode = shareCodeInput.text.ToUpper().Trim();

            if (string.IsNullOrWhiteSpace(shareCode))
            {
                Debug.LogWarning("[LoadAvatar] missing input");
                return;
            }

            Debug.Log($"[DB] searching for avatar with: {shareCode}...");
            progressText.text = "Status: Finding Avatar...";

            var avatarData = await DBManager.Instance.GetAvatarByShareCode(shareCode);
            
            if (avatarData != null)
            {
                // --- DAILY PROGRESS CHECK ---
                DateTime today_local = DateTime.Now;
                DateTime last_logged_utc = DateTime.Parse(avatarData.last_logged, null, System.Globalization.DateTimeStyles.RoundtripKind);
                DateTime last_logged_local = last_logged_utc.ToLocalTime();

                if (today_local.Date > last_logged_local.Date)
                {
                    Debug.Log("[Login] new day detected, incrementing days_completed");
                    int new_days = avatarData.days_completed + 1;
                    await DBManager.Instance.UpdateAvatarProgress(shareCode, new_days, DateTime.UtcNow);
                    avatarData.days_completed = new_days;
                }

                ProceedToUserHome(avatarData);
            }
            else
            {
                Debug.LogError($"[DB] no avatar found with: {shareCode}");
                progressText.text = "Error: Invalid share code";
            }
        }

        private void ProceedToUserHome(DBManager.AvatarData avatarData)
        {
            AvatarManager.Instance.SetCurrentAvatar(avatarData.url, avatarData.name, avatarData.days_completed);            
            SceneManager.LoadScene("UserHome");
        }

        // --- DEBUGGING: reset checklist ---
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                PlayerPrefs.DeleteKey("LastChecklistDate");
                PlayerPrefs.Save();
                Debug.LogWarning("--- PlayerPrefs 'LastChecklistDate' RESET ---");
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
                string original_url = message.Args["url"];
                Debug.Log("[Server] original url: " + original_url);

                webView.Hide();
                getAvatarButton.interactable = false;
                progressText.text = "Status: Modifying Avatar using Blender...";

                // start new server-side processing
                _ = ServerProcessing(original_url);
            }
        }

        private async Task ServerProcessing(string originalUrl)
        {
            string server_api = "http://ec2-51-20-107-68.eu-north-1.compute.amazonaws.com:5000/process-avatar";
            // string server_api = "http://10.74.130.118:5000/process-avatar"; // dev mode
            
            var request_data = new AvatarProcessRequest { url = originalUrl };
            string json_body = JsonUtility.ToJson(request_data);
            byte[] raw_body = Encoding.UTF8.GetBytes(json_body);

            using (UnityWebRequest www = new UnityWebRequest(server_api, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(raw_body);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                try
                {
                    await www.SendWebRequest();
                }
                catch (Exception e)
                {
                    Debug.LogError("[Server-Error] " + e.Message);
                    progressText.text = "Status: Failed to modify Avatar";
                    getAvatarButton.interactable = true;
                    return;
                }
            
                string response_json = www.downloadHandler.text;
                var response_data = JsonUtility.FromJson<AvatarProcessResponse>(response_json);
                string modified_url = response_data.newUrl;

                Debug.Log("[Server] modified url: " + modified_url);
                progressText.text = "Status: Loading modified Avatar...";

                bool loaded = await metaPersonLoader.LoadModelAsync(modified_url, p => progressText.text = string.Format("Downloading: {0}%", (int)(p * 100)));
                
                if (loaded)
                {
                    progressText.text = string.Empty;
                    importControls.SetActive(false);
                    AvatarManager.Instance.SetCurrentAvatar(modified_url, "New Avatar");
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