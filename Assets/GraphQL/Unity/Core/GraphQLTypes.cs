using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GraphQL.Unity.Core
{
    /// <summary>
    /// Represents a GraphQL request to be sent to a server.
    /// </summary>
    [Serializable]
    public class GraphQLRequest
    {
        [JsonProperty("query")]
        public string query;

        [JsonProperty("operationName")]
        public string operationName;

        [JsonProperty("variables")]
        public Dictionary<string, object> variables;

        [JsonIgnore]
        public bool UseCache { get; set; }

        public GraphQLRequest(string query, string operationName = null, Dictionary<string, object> variables = null, bool useCache = false)
        {
            this.query = query;
            this.operationName = operationName;
            this.variables = variables;
            this.UseCache = useCache;
        }
    }

    /// <summary>
    /// Represents an error returned in a GraphQL response.
    /// </summary>
    [Serializable]
    public class GraphQLError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("locations")]
        public List<GraphQLErrorLocation> Locations { get; set; }

        [JsonProperty("path")]
        public List<string> Path { get; set; }

        [JsonProperty("extensions")]
        public Dictionary<string, object> Extensions { get; set; }
    }

    /// <summary>
    /// Represents the location of an error in a GraphQL query.
    /// </summary>
    [Serializable]
    public class GraphQLErrorLocation
    {
        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("column")]
        public int Column { get; set; }
    }

    /// <summary>
    /// Represents a response from a GraphQL server.
    /// </summary>
    [Serializable]
    public class GraphQLResponse<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("errors")]
        public List<GraphQLError> Errors { get; set; }

        [JsonProperty("pageInfo")]
        public PaginationInfo PageInfo { get; set; }

        public bool HasErrors => Errors != null && Errors.Count > 0;
    }

    /// <summary>
    /// Represents an exception that occurred during a GraphQL operation.
    /// </summary>
    public class GraphQLException : Exception
    {
        public List<GraphQLError> Errors { get; }

        public GraphQLException(string message, List<GraphQLError> errors) : base(message)
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// Provides extension methods for GraphQL error handling.
    /// </summary>
    public static class GraphQLErrorExtensions
    {
        public static string GetErrorSummary(this List<GraphQLError> errors)
        {
            if (errors == null || errors.Count == 0)
                return "No errors";

            return string.Join("; ", errors.ConvertAll(error => error.Message));
        }
    }

    /// <summary>
    /// Represents pagination information for GraphQL queries.
    /// </summary>
    [Serializable]
    public class PaginationInfo
    {
        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("hasPreviousPage")]
        public bool HasPreviousPage { get; set; }

        [JsonProperty("startCursor")]
        public string StartCursor { get; set; }

        [JsonProperty("endCursor")]
        public string EndCursor { get; set; }
    }

    /// <summary>
    /// Represents a paginated GraphQL query.
    /// </summary>
    public class PaginatedQuery<T>
    {
        public GraphQLRequest Query { get; set; }
        public int PageSize { get; set; }
        public string After { get; set; }
        public string Before { get; set; }

        public PaginatedQuery(GraphQLRequest query, int pageSize)
        {
            Query = query;
            PageSize = pageSize;
        }
    }

}