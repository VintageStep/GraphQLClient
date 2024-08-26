using System.Threading.Tasks;

namespace GraphQL.Unity
{
    /// <summary>
    /// Represents an interface for making web requests, particularly designed for use with GraphQL operations.
    /// </summary>
    public interface IWebRequest
    {
        /// <summary>
        /// Sets a header for the web request.
        /// </summary>
        /// <param name="name">The name of the header.</param>
        /// <param name="value">The value of the header.</param>
        void SetRequestHeader(string name, string value);

        /// <summary>
        /// Sets the body content of the web request.
        /// </summary>
        /// <param name="body">The string representation of the request body, typically a JSON-formatted GraphQL query.</param>
        void SetRequestBody(string body);

        /// <summary>
        /// Sends the web request asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response body as a string.</returns>
        Task<string> SendWebRequestAsync();
    }
}