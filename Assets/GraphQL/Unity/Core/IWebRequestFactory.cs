namespace GraphQL.Unity
{
    /// <summary>
    /// Represents a factory for creating IWebRequest instances.
    /// This interface allows for the creation of web requests with specific configurations.
    /// </summary>
    public interface IWebRequestFactory
    {
        /// <summary>
        /// Creates a new instance of IWebRequest.
        /// </summary>
        /// <param name="url">The URL for the web request.</param>
        /// <param name="method">The HTTP method for the request (e.g., GET, POST, PUT, DELETE).</param>
        /// <returns>An instance of IWebRequest configured with the specified URL and HTTP method.</returns>
        IWebRequest CreateWebRequest(string url, string method);
    }
}
