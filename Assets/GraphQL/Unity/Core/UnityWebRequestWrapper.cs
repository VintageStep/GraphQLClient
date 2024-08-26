using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;

namespace GraphQL.Unity
{
    /// <summary>
    /// Wrapper class for UnityWebRequest to implement the IWebRequest interface.
    /// This class is used to mock Unity's networking functionality in tests.
    /// </summary>
    public class UnityWebRequestWrapper : IWebRequest
    {
        private readonly UnityWebRequest _webRequest;

        /// <summary>
        /// Initializes a new instance of the UnityWebRequestWrapper class.
        /// </summary>
        /// <param name="url">The URL for the web request.</param>
        /// <param name="method">The HTTP method for the request.</param>
        public UnityWebRequestWrapper(string url, string method)
        {
            _webRequest = new UnityWebRequest(url, method);
            _webRequest.downloadHandler = new DownloadHandlerBuffer();
        }

        /// <summary>
        /// Sets a header for the web request.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        public void SetRequestHeader(string name, string value)
        {
            _webRequest.SetRequestHeader(name, value);
        }

        /// <summary>
        /// Sets the body of the web request.
        /// </summary>
        /// <param name="body">The body content as a string.</param>
        public void SetRequestBody(string body)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
            _webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        /// <summary>
        /// Sends the web request asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response as a string.</returns>
        public async Task<string> SendWebRequestAsync()
        {
            await _webRequest.SendWebRequest();
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                throw new System.Exception($"Network error: {_webRequest.error}");
            }
            return _webRequest.downloadHandler.text;
        }
    }
}