using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.Networking;

// helper method to make UnityWebRequest awaitable
public static class UnityWebRequestAwaiter
{
    public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        var tcs = new TaskCompletionSource<object>();
        asyncOp.completed += _ => { tcs.TrySetResult(null); };
        return ((Task)tcs.Task).GetAwaiter();
    }
}