using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System;

public class DBManager : MonoBehaviour
{
    public static DBManager Instance { get; private set; }
    private string dbUrl;
    private string dbAnonKey;

    [System.Serializable]
    public class AvatarData
    {
        public string name, url, share_code, last_logged;
        public int days_completed;
    }

    [System.Serializable]
    private class PostData
    {
        public string name, url, share_code;
        public int days_completed = 0;
    }

    [System.Serializable] private class AvatarList { public AvatarData[] avatars; }

    [System.Serializable]
    private class UpdateData
    {
        public int days_completed;
        public string last_logged;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        EnvLoader.LoadEnv();
        dbUrl = EnvLoader.Get("SUPABASE_URL");
        dbAnonKey = EnvLoader.Get("SUPABASE_ANON_KEY");
    }

    private string GenerateShareCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        char[] code = new char[6];
        for (int i = 0; i < code.Length; i++)
        {
            code[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        }
        return new string(code);
    }
    
    public async Task<string> SaveAvatar(string avatarName, string avatarUrl)
    {
        string generatedCode = null;
        int maxRetries = 5;

        for (int i = 0; i < maxRetries; i++)
        {
            generatedCode = GenerateShareCode();
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(PostAvatarData(avatarName, avatarUrl, generatedCode, (isSuccess) => tcs.SetResult(isSuccess)));
            bool success = await tcs.Task;

            if (success) return generatedCode;
        }

        Debug.LogError($"[DBManager] failed to save avatar after {maxRetries} attempts");
        return null;
    }

    public async Task<AvatarData> GetAvatarByShareCode(string shareCode)
    {
        var tcs = new TaskCompletionSource<AvatarData>();
        StartCoroutine(FetchAvatarData(shareCode.ToUpper(), (avatar) => tcs.SetResult(avatar)));
        return await tcs.Task;
    }

    public async Task<bool> UpdateAvatarProgress(string shareCode, int newDaysCompleted, DateTime newLoginDate)
    {
        var tcs = new TaskCompletionSource<bool>();
        StartCoroutine(PatchAvatarData(shareCode, newDaysCompleted, newLoginDate, (isSuccess) => tcs.SetResult(isSuccess)));
        return await tcs.Task;
    }

    private IEnumerator PatchAvatarData(string shareCode, int days, DateTime date, System.Action<bool> callback)
    {
        // require: dates in iso 8601 format
        string isoDate = date.ToUniversalTime().ToString("o");
        var data = new UpdateData { days_completed = days, last_logged = isoDate };
        string json = JsonUtility.ToJson(data);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

        string requestUrl = $"{dbUrl}/rest/v1/avatars?share_code=eq.{shareCode}";

        using (UnityWebRequest request = new UnityWebRequest(requestUrl, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", dbAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {dbAnonKey}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();
            
            if (request.responseCode != 204) // PATCH success
            {
                Debug.LogError($"ERROR: [DBManager] {request.error} | {request.downloadHandler.text}");
            }
            callback(request.responseCode == 204);
        }
    }

    private IEnumerator PostAvatarData(string name, string url, string code, System.Action<bool> callback)
    {
        var data = new PostData { name = name, url = url, share_code = code };
        string json = JsonUtility.ToJson(data);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);
        string requestUrl = $"{dbUrl}/rest/v1/avatars";

        using (UnityWebRequest request = new UnityWebRequest(requestUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", dbAnonKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            callback(request.responseCode == 201);
        }
    }
    
    private IEnumerator FetchAvatarData(string code, System.Action<AvatarData> callback)
    {
        string requestUrl = $"{dbUrl}/rest/v1/avatars?share_code=eq.{code}&select=*&limit=1";
        using (UnityWebRequest request = new UnityWebRequest(requestUrl, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("apikey", dbAnonKey);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string fixedJson = "{\"avatars\":" + request.downloadHandler.text + "}";
                AvatarList result = JsonUtility.FromJson<AvatarList>(fixedJson);
                callback(result.avatars != null && result.avatars.Length > 0 ? result.avatars[0] : null);
            }
            else
            {
                callback(null);
            }
        }
    }
}