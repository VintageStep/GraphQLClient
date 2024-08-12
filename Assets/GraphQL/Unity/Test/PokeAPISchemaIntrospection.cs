using UnityEngine;
using System.Collections.Generic;
using GraphQL.Unity.Core;
using System;
using Newtonsoft.Json;
using System.IO;

using System.Linq;

namespace GraphQL.Unity.Test
{

    public class PokeAPISchemaIntrospection : MonoBehaviour
    {
        public GraphQLClientBehaviour graphQLClient;

        // Start is called before the first frame update
        void Start()
        {
            FetchAndSaveSchema();
        }

        // Schema related 

        void FetchAndSaveSchema()
        {
            var query = new QueryBuilder()
                .Operation("IntrospectionQuery")
                .BeginObject("__schema")
                    .BeginObject("types")
                        .Field("name")
                        .Field("kind")
                        .BeginObject("fields")
                            .Field("name")
                            .BeginObject("type")
                                .Field("name")
                                .Field("kind")
                                .BeginObject("ofType")
                                    .Field("name")
                                    .Field("kind")
                                .EndObject()
                            .EndObject()
                            .BeginObject("args")
                                .Field("name")
                                .BeginObject("type")
                                    .Field("name")
                                    .Field("kind")
                                    .BeginObject("ofType")
                                        .Field("name")
                                        .Field("kind")
                                    .EndObject()
                                .EndObject()
                            .EndObject()
                        .EndObject()
                    .EndObject()
                    .BeginObject("queryType")
                        .Field("name")
                        .BeginObject("fields")
                            .Field("name")
                            .BeginObject("type")
                                .Field("name")
                                .Field("kind")
                                .BeginObject("ofType")
                                    .Field("name")
                                    .Field("kind")
                                .EndObject()
                            .EndObject()
                            .BeginObject("args")
                                .Field("name")
                                .BeginObject("type")
                                    .Field("name")
                                    .Field("kind")
                                    .BeginObject("ofType")
                                        .Field("name")
                                        .Field("kind")
                                    .EndObject()
                                .EndObject()
                            .EndObject()
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log("Sending introspection query to fetch schema...");

            graphQLClient.SendQuery<SchemaResponse>(query, (response, error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"Schema query failed: {error.Message}");
                    return;
                }

                if (response.Data != null)
                {
                    SaveSchemaToFile(response.Data);
                }
                else
                {
                    Debug.LogWarning("No schema Data received");
                }
            });
        }

        void SaveSchemaToFile(SchemaResponse schemaData)
        {
            var relevantInfo = AnalyzeSchema(schemaData);
            string json = JsonConvert.SerializeObject(relevantInfo, Formatting.Indented);
            string directoryPath = Path.Combine(Application.dataPath, "GraphQL", "Unity", "Test");
            string filePath = Path.Combine(directoryPath, "relevant_schema.json");

            try
            {
                Directory.CreateDirectory(directoryPath);
                File.WriteAllText(filePath, json);
                Debug.Log($"Relevant schema info saved to {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save relevant schema info: {e.Message}");
            }
        }

        RelevantSchemaInfo AnalyzeSchema(SchemaResponse schemaData)
        {
            var relevantInfo = new RelevantSchemaInfo();
            var relevantTypeNames = new HashSet<string> {
                "Pokemon", "Move", "Type", "Ability", "Generation", "Region",
                "PokemonSpecies", "PokemonForm", "VersionGroup"
            };

            foreach (var type in schemaData.Schema.Types)
            {
                if (relevantTypeNames.Contains(type.Name))
                {
                    relevantInfo.TypeFields[type.Name] = type.Fields?
                        .Select(f => new TypeFieldInfo
                        {
                            Name = f.Name,
                            Type = GetFullTypeName(f.Type),
                            Args = f.Args?.Select(a => new ArgInfo
                            {
                                Name = a.Name,
                                Type = GetFullTypeName(a.Type)
                            }).ToList() ?? new List<ArgInfo>()
                        })
                        .ToList() ?? new List<TypeFieldInfo>();
                }
            }

            relevantInfo.QueryFields = schemaData.Schema.QueryType.Fields
                .Where(f => f.Name.StartsWith("pokemon_") || f.Name.StartsWith("move_") ||
                            f.Name.StartsWith("type_") || f.Name.StartsWith("ability_") ||
                            f.Name.StartsWith("generation_") || f.Name.StartsWith("region_"))
                .Select(f => new TypeFieldInfo
                {
                    Name = f.Name,
                    Type = GetFullTypeName(f.Type),
                    Args = f.Args?.Select(a => new ArgInfo
                    {
                        Name = a.Name,
                        Type = GetFullTypeName(a.Type)
                    }).ToList() ?? new List<ArgInfo>()
                })
                .ToList();

            return relevantInfo;
        }

        string GetFullTypeName(TypeRef type)
        {
            if (type == null) return "Unknown";

            string typeName = type.Name ?? "Unknown";
            switch (type.Kind)
            {
                case "NON_NULL":
                    return $"{GetFullTypeName(type.OfType)}!";
                case "LIST":
                    return $"[{GetFullTypeName(type.OfType)}]";
                default:
                    return typeName;
            }
        }


    }

    // Schema types etc

    [Serializable]
    public class SchemaResponse
    {
        [JsonProperty("__schema")]
        public SchemaData Schema { get; set; }
    }

    [Serializable]
    public class SchemaData
    {
        [JsonProperty("types")]
        public List<TypeData> Types { get; set; }

        [JsonProperty("queryType")]
        public QueryTypeData QueryType { get; set; }
    }

    [Serializable]
    public class TypeData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("fields")]
        public List<FieldData> Fields { get; set; }
    }

    [Serializable]
    public class FieldData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public TypeRef Type { get; set; }

        [JsonProperty("args")]
        public List<ArgumentData> Args { get; set; }
    }

    [Serializable]
    public class TypeRef
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("ofType")]
        public TypeRef OfType { get; set; }
    }

    [Serializable]
    public class QueryTypeData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fields")]
        public List<QueryFieldData> Fields { get; set; }
    }

    [Serializable]
    public class QueryFieldData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public TypeRef Type { get; set; }

        [JsonProperty("args")]
        public List<ArgumentData> Args { get; set; }
    }

    [Serializable]
    public class ArgumentData
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public TypeRef Type { get; set; }
    }

    public class TypeFieldInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<ArgInfo> Args { get; set; } = new List<ArgInfo>();
    }

    public class ArgInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class RelevantSchemaInfo
    {
        public Dictionary<string, List<TypeFieldInfo>> TypeFields { get; set; } = new Dictionary<string, List<TypeFieldInfo>>();
        public List<TypeFieldInfo> QueryFields { get; set; } = new List<TypeFieldInfo>();
    }

}