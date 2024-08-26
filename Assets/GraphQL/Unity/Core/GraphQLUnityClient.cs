using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;

namespace GraphQL.Unity
{
    /// <summary>
    /// Provides a client for making GraphQL requests in Unity applications.
    /// </summary>
    public class GraphQLUnityClient
    {
        private readonly string _url;
        private readonly string _authToken;
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        private readonly IWebRequestFactory _webRequestFactory;

        /// <summary>
        /// Initializes a new instance of the GraphQLUnityClient.
        /// </summary>
        /// <param name="url">The URL of the GraphQL endpoint.</param>
        /// <param name="authToken">Optional authentication token, null by default.</param>
        public GraphQLUnityClient(string url, string authToken = null, IWebRequestFactory webRequestFactory = null)
        {
            _url = url;
            _authToken = authToken;
            _webRequestFactory = webRequestFactory ?? new UnityWebRequestFactory();
        }

        /// <summary>
        /// Method to access a copy of the URL of the GraphQL endpoint.
        /// </summary>
        /// <returns> A String of the assigned url</returns>
        public string Url => _url;

        /// <summary>
        /// Sends a GraphQL query asynchronously and returns the response.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response data.</typeparam>
        /// <param name="request">The GraphQL request to send.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the GraphQL response.</returns>
        public async Task<GraphQLResponse<TResponse>> SendQueryAsync<TResponse>(GraphQLRequest request)
        {
            string cacheKey = GenerateCacheKey(request);
            if (request.UseCache && _cache.TryGetValue(cacheKey, out object cachedResponse))
            {
                return (GraphQLResponse<TResponse>)cachedResponse;
            }

            var requestObject = new
            {
                query = request.query,
                operationName = request.operationName,
                variables = request.variables != null && request.variables.Count > 0 ? request.variables : null
            };

            string jsonPayload = JsonHelper.Serialize(requestObject);
            //Debug.Log($"Sending request: {jsonPayload}");

            var webRequest = _webRequestFactory.CreateWebRequest(_url, "POST");

            // TODO test headers and auth token
            webRequest.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(_authToken))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            }
            webRequest.SetRequestBody(jsonPayload);

            try
            {
                string responseJson = await webRequest.SendWebRequestAsync();
                //Debug.Log($"Raw response: {responseJson}");

                GraphQLResponse<TResponse> response = JsonHelper.Deserialize<GraphQLResponse<TResponse>>(responseJson);

                if (response.HasErrors)
                {
                    throw new GraphQLException($"GraphQL errors: {response.Errors.GetErrorSummary()}", response.Errors);
                }

                if (request.UseCache)
                {
                    _cache[cacheKey] = response;
                }

                return response;
            }
            catch (Exception ex) when (!(ex is GraphQLException))
            {
                throw new GraphQLException($"Unexpected error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Generates a cache key for a GraphQL request.
        /// </summary>
        /// <param name="request">The GraphQL request.</param>
        /// <returns>A string that can be used as a cache key.</returns>
        private string GenerateCacheKey(GraphQLRequest request)
        {
            return JsonHelper.Serialize(new { request.query, request.variables });
        }
    }

}