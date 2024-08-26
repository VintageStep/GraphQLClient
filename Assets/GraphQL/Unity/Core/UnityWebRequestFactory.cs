namespace GraphQL.Unity
{
    /// <summary>
    /// Factory class for creating IWebRequest instances.
    /// This class is used to create UnityWebRequestWrapper instances in the actual implementation,
    /// and can be mocked in tests to provide custom IWebRequest implementations.
    /// </summary>
    public class UnityWebRequestFactory : IWebRequestFactory
    {
        /// <summary>
        /// Creates a new IWebRequest instance.
        /// </summary>
        /// <param name="url">The URL for the web request.</param>
        /// <param name="method">The HTTP method for the request.</param>
        /// <returns>An IWebRequest instance.</returns>
        public IWebRequest CreateWebRequest(string url, string method)
        {
            return new UnityWebRequestWrapper(url, method);
        }
    }
}