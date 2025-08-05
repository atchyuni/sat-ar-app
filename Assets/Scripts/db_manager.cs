using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

public class DBManager : MonoBehaviour
{
    public static DBManager Instance { get; private set; }

    // --- SUPABASE CREDENTIALS ---
    private string dbUrl = "https://yrnfgbeqrnltwogussiz.supabase.co";
    private string dbAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InlybmZnYmVxcm5sdHdvZ3Vzc2l6Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTQzMTgzMDgsImV4cCI6MjA2OTg5NDMwOH0.IbeCfEmTrLa4SmXapDWXP6ipxp_njQpIsJc2nXLv6-k";

    [System.Serializable]
    public class AvatarData
    {
        public string name;
        public string url;
        public string share_code;
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
    }

    private string GenerateShareCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        char[] code = new char[6];
        for (int i = 0; i < code.Length; i++)
        {
            code[i] = chars[Random.Range(0, chars.Length)];
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

        Debug.LogError($"failed to save avatar after {maxRetries} attempts");
        return null;
    }

    public async Task<AvatarData> GetAvatarByShareCode(string shareCode)
    {
        var tcs = new TaskCompletionSource<AvatarData>();
        StartCoroutine(FetchAvatarData(shareCode.ToUpper(), (avatar) => tcs.SetResult(avatar)));
        return await tcs.Task;
    }

    [System.Serializable] private class PostData { public string name, url, share_code; }
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
    
    [System.Serializable] private class AvatarList { public AvatarData[] avatars; }
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