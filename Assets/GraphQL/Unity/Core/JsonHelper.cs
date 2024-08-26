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
        private static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializerSettings StrictSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error
        };

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, DefaultSettings);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="strict">By fedault false, if true JsonHelper will throw an exception when the JSON doesn't exactly match the expected structure.</param>
        /// <returns>An object of type T deserialized from the JSON string.</returns>
        public static T Deserialize<T>(string json, bool strict = false)
        {
            return JsonConvert.DeserializeObject<T>(json, strict ? StrictSettings : DefaultSettings);
        }
    }
}