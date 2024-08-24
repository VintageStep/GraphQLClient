using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GraphQL.Unity
{
    /// <summary>
    /// Provides helper methods for JSON serialization and deserialization using Newtonsoft.Json.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// JSON serializer settings used for all serialization and deserialization operations.
        /// </summary>
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An object of type T deserialized from the JSON string.</returns>
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}