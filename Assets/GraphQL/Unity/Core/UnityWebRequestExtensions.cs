using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GraphQL.Unity.Core
{
    /// <summary>
    /// Provides extension methods for UnityWebRequestAsyncOperation to enable async/await pattern usage.
    /// Original snippet: https://gist.github.com/mattyellen/d63f1f557d08f7254345bff77bfdc8b3
    /// </summary>
    public static class UnityWebRequestExtensions
    {
        /// <summary>
        /// Extends UnityWebRequestAsyncOperation to be awaitable using the async/await pattern.
        /// </summary>
        /// <param name="asyncOp">The UnityWebRequestAsyncOperation to make awaitable.</param>
        /// <returns>A TaskAwaiter that can be used in an await expression.</returns>
        public static TaskAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }
}

/* Example:
var getRequest = UnityWebRequest.Get("http://www.google.com");
await getRequest.SendWebRequest();
var result = getRequest.downloadHandler.text;
*/