using UnityEngine;
using System;
using System.Threading.Tasks;

namespace GraphQL.Unity
{
    /// <summary>
    /// A MonoBehaviour wrapper for the GraphQLUnityClient, allowing it to be attached to GameObjects and configured in the Unity Inspector.
    /// </summary>
    public class GraphQLClientBehaviour : MonoBehaviour
    {
        [SerializeField] private string graphQLUrl = "https://api.example.com/graphql";
        [SerializeField] private string authToken;

        private GraphQLUnityClient _client;

        /// <summary>
        /// Gets the GraphQLUnityClient instance, creating it if it doesn't exist.
        /// </summary>
        public GraphQLUnityClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new GraphQLUnityClient(graphQLUrl, authToken);
                }
                return _client;
            }
        }

        /// <summary>
        /// Sends a GraphQL query asynchronously and invokes a callback with the result.
        /// </summary>
        /// <typeparam name="TResponse">The type of the expected response data.</typeparam>
        /// <param name="request">The GraphQL request to send.</param>
        /// <param name="callback">A callback to handle the response or any errors.</param>
        public async void SendQuery<TResponse>(GraphQLRequest request, Action<GraphQLResponse<TResponse>, Exception> callback)
        {
            try
            {
                var response = await Client.SendQueryAsync<TResponse>(request);
                callback(response, null);
            }
            catch (GraphQLException ex)
            {
                Debug.LogError($"GraphQL error: {ex.Message}");
                if (ex.Errors != null)
                {
                    foreach (var error in ex.Errors)
                    {
                        Debug.LogError($"- {error.Message}");
                    }
                }
                callback(null, ex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex.Message}");
                callback(null, ex);
            }
        }

    }
}