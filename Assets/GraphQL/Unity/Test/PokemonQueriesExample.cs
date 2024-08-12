using UnityEngine;
using System.Collections.Generic;
using GraphQL.Unity.Core;
using System;
using Newtonsoft.Json;

namespace GraphQL.Unity.Test
{
    public class PokemonGenerationExample : MonoBehaviour
    {
        public GraphQLClientBehaviour graphQLClient;

        void Start()
        {
            // they work!

            SendPokemonGenerationQuery();
            //SendPokemonDetailQuery();
            //SendPokemonLocationQuery();
            //SendPokemonPokedexEntriesQuery();

            // veryfying schema 

            //FetchAndSaveSchema();

            // Second test 

            //SendPokemonsByTypeAndGenQuery();
            //SendPokemonHiddenAbilityQuery();
            //SendDualTypePokemonInGenQuery();
            //SendPokemonNativeRegionQuery();
            SendMovesWithAilmentQuery();
            //SendMovesLearnedByPokemonQuery();  //Comented in order to move the types into the demo, remove the types from the demo and uncomment them here if needed
            //SendMovesWithAccuracyAndTypeQuery();
        }

        void SendPokemonGenerationQuery()
        {
            var query = new QueryBuilder()
                .Operation("AmountOfPokemonByGen")
                .BeginObject("generations: pokemon_v2_generation")
                    .Field("name")
                    .BeginObject("pokemon_species: pokemon_v2_pokemonspecies_aggregate")
                        .BeginObject("aggregate")
                            .Field("count")
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            query.UseCache = true; // Enable caching for this query

            Debug.Log($"Sending generation query: {query.query}");

            graphQLClient.SendQuery<PokemonGenerationResponse>(query, HandleGenerationResponse);
        }

        void SendPokemonDetailQuery()
        {
            var query = new QueryBuilder()
                .Operation("PokemonDetail")
                .Variable("name", "pikachu", "String")
                .BeginObject("pokemon: pokemon_v2_pokemon(where: {name: {_eq: $name}})")
                    .Field("id")
                    .Field("name")
                    .BeginObject("types: pokemon_v2_pokemontypes")
                        .BeginObject("type: pokemon_v2_type")
                            .Field("name")
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending detail query: {query.query}");

            graphQLClient.SendQuery<PokemonDetailResponse>(query, HandleDetailResponse);
        }


        void HandleGenerationResponse(GraphQLResponse<PokemonGenerationResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"Generation query failed: {error.Message}");
                return;
            }

            if (response.Data?.Generations != null)
            {
                Debug.Log("Pokemon Generations:");
                foreach (var generation in response.Data.Generations)
                {
                    Debug.Log($"{generation.Name}: {generation.PokemonSpecies.Aggregate.Count} species");
                }
            }
            else
            {
                Debug.LogWarning("No generation Data received");
            }
        }

        void HandleDetailResponse(GraphQLResponse<PokemonDetailResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"Detail query failed: {error.Message}");
                return;
            }

            if (response.Data != null && response.Data.pokemon != null && response.Data.pokemon.Count > 0)
            {
                var pokemon = response.Data.pokemon[0];
                Debug.Log($"Pokemon Detail: {pokemon.name} (ID: {pokemon.id})");
                Debug.Log("Types:");
                foreach (var typeInfo in pokemon.types)
                {
                    Debug.Log($"- {typeInfo.type.Name}");
                }
            }
            else
            {
                Debug.LogWarning("No pokemon detail Data received");
            }
        }

        void SendPokemonLocationQuery()
        {
            var query = new QueryBuilder()
                .Operation("PokemonAvailableInLocationArea")
                .Variable("location", "sinnoh-route-203-area", "String")
                .BeginObject("location: pokemon_v2_locationarea(where: {name: {_eq: $location}})")
                    .Field("name")
                    .BeginObject("pokemon_v2_encounters")
                        .BeginObject("pokemon_v2_pokemon")
                            .Field("name")
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending location query: {query.query}");

            graphQLClient.SendQuery<PokemonLocationResponse>(query, HandleLocationResponse);
        }

        void HandleLocationResponse(GraphQLResponse<PokemonLocationResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"Location query failed: {error.Message}");
                return;
            }

            if (response.Data != null && response.Data.Location != null && response.Data.Location.Count > 0)
            {
                var location = response.Data.Location[0];
                Debug.Log($"Pokemon in {location.Name}:");
                foreach (var encounter in location.PokemonV2Encounters)
                {
                    Debug.Log($"- {encounter.PokemonV2Pokemon.Name}");
                }
            }
            else
            {
                Debug.LogWarning("No location Data received");
            }
        }

        void SendPokemonPokedexEntriesQuery()
        {
            var query = new QueryBuilder()
                .Operation("PokemonPokedexEntriesByIdAndLanguage")
                .Variable("id", 4, "Int!")
                .Variable("lang", "en", "String")
                .BeginObject("pokemon_v2_pokemonspeciesflavortext(where: {id: {}, pokemon_species_id: {_eq: $id}, pokemon_v2_language: {name: {_eq: $lang}}})")
                    .Field("flavor_text")
                    .Field("pokemon_species_id")
                    .BeginObject("pokemon_v2_version")
                        .Field("name")
                        .Field("id")
                    .EndObject()
                    .BeginObject("pokemon_v2_language")
                        .Field("name")
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending pokedex entries query: {query.query}");

            graphQLClient.SendQuery<PokemonPokedexEntriesResponse>(query, HandlePokedexEntriesResponse);
        }

        void HandlePokedexEntriesResponse(GraphQLResponse<PokemonPokedexEntriesResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"Pokedex entries query failed: {error.Message}");
                return;
            }

            if (response.Data != null && response.Data.PokemonV2PokemonspeciesflavorText != null && response.Data.PokemonV2PokemonspeciesflavorText.Count > 0)
            {
                foreach (var entry in response.Data.PokemonV2PokemonspeciesflavorText)
                {
                    Debug.Log($"Pokedex Entry (Species ID: {entry.PokemonSpeciesId}, Version: {entry.PokemonV2Version.Name}, Language: {entry.PokemonV2Language.Name}):");
                    Debug.Log(entry.FlavorText);
                }
            }
            else
            {
                Debug.LogWarning("No pokedex entries received");
            }
        }


        /// SECOND TEST 

        void SendPokemonsByTypeAndGenQuery()
        {
            var query = new QueryBuilder()
                .Operation("PokemonsByTypeAndGen")
                .Variable("type", "water", "String!")
                .Variable("generation", 1, "Int!")
                .BeginObject("pokemon_v2_pokemon(where: {pokemon_v2_pokemontypes: {pokemon_v2_type: {name: {_eq: $type}}}, pokemon_v2_pokemonspecy: {generation_id: {_eq: $generation}}})")
                    .Field("name")
                    .BeginObject("pokemon_v2_pokemonspecy")
                        .Field("generation_id")
                    .EndObject()
                    .BeginObject("pokemon_v2_pokemontypes")
                        .BeginObject("pokemon_v2_type")
                            .Field("name")
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending PokemonsByTypeAndGen query: {query.query}");
            graphQLClient.SendQuery<PokemonsByTypeAndGenResponse>(query, HandlePokemonsByTypeAndGenResponse);
        }

        void HandlePokemonsByTypeAndGenResponse(GraphQLResponse<PokemonsByTypeAndGenResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"PokemonsByTypeAndGen query failed: {error.Message}");
                return;
            }

            if (response.Data != null && response.Data.pokemon_v2_pokemon != null)
            {
                Debug.Log("Pokemons by Type and Generation:");
                foreach (var pokemon in response.Data.pokemon_v2_pokemon)
                {
                    Debug.Log($"- {pokemon.name} (Gen: {pokemon.pokemon_v2_pokemonspecy.generation_id})");
                }
            }
            else
            {
                Debug.LogWarning("No PokemonsByTypeAndGen Data received");
            }
        }

        void SendPokemonHiddenAbilityQuery()
        {
            var query = new QueryBuilder()
                .Operation("PokemonHiddenAbility")
                .Variable("pokemonName", "charizard", "String!")
                .BeginObject("pokemon_v2_pokemonability(where: {is_hidden: {_eq: true}, pokemon_v2_pokemon: {name: {_eq: $pokemonName}}})")
                    .BeginObject("pokemon_v2_ability")
                        .Field("name")
                    .EndObject()
                    .BeginObject("pokemon_v2_pokemon")
                        .Field("name")
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending PokemonHiddenAbility query: {query.query}");
            graphQLClient.SendQuery<PokemonHiddenAbilityResponse>(query, HandlePokemonHiddenAbilityResponse);
        }

        void HandlePokemonHiddenAbilityResponse(GraphQLResponse<PokemonHiddenAbilityResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"PokemonHiddenAbility query failed: {error.Message}");
                return;
            }

            if (response.Data != null && response.Data.pokemon_v2_pokemonability.Count > 0)
            {
                var ability = response.Data.pokemon_v2_pokemonability[0];
                Debug.Log($"Hidden Ability of {ability.pokemon_v2_pokemon.name}: {ability.pokemon_v2_ability.name}");
            }
            else
            {
                Debug.LogWarning("No PokemonHiddenAbility Data received");
            }
        }

        void SendPokemonNativeRegionQuery()
        {
            var query = new QueryBuilder()
                .Operation("PokemonNativeRegion")
                .Variable("pokemonName", "garchomp", "String!")
                .BeginObject("pokemon_v2_pokemonspecies(where: {name: {_eq: $pokemonName}})")
                    .Field("name")
                    .BeginObject("pokemon_v2_generation")
                        .BeginObject("pokemon_v2_region")
                            .Field("name")
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending PokemonNativeRegion query: {query.query}");
            graphQLClient.SendQuery<PokemonNativeRegionResponse>(query, HandlePokemonNativeRegionResponse);
        }

        void HandlePokemonNativeRegionResponse(GraphQLResponse<PokemonNativeRegionResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"PokemonNativeRegion query failed: {error.Message}");
                return;
            }

            if (response.Data?.PokemonV2PokemonSpecies != null && response.Data.PokemonV2PokemonSpecies.Count > 0)
            {
                var pokemon = response.Data.PokemonV2PokemonSpecies[0];
                Debug.Log($"{pokemon.Name}'s native region: {pokemon.Generation.Region.Name}");
            }
            else
            {
                Debug.LogWarning("No PokemonNativeRegion Data received");
            }
        }

        void SendMovesWithAilmentQuery()
        {
            var query = new QueryBuilder()
                .Operation("MovesWithAilment")
                .Variable("ailment", "sleep", "String!")
                .BeginObject("pokemon_v2_move(where: {pokemon_v2_movemeta: {pokemon_v2_movemetaailment: {name: {_ilike: $ailment}}}})")
                    .Field("name")
                    .BeginObject("pokemon_v2_movemeta")
                        .BeginObject("pokemon_v2_movemetaailment")
                            .Field("name")
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending MovesWithAilment query: {query.query}");
            graphQLClient.SendQuery<MovesWithAilmentResponse>(query, HandleMovesWithAilmentResponse);
        }

        void HandleMovesWithAilmentResponse(GraphQLResponse<MovesWithAilmentResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"MovesWithAilment query failed: {error.Message}");
                return;
            }

            if (response.Data?.Moves != null)
            {
                Debug.Log("Moves that cause sleep:");
                foreach (var move in response.Data.Moves)
                {
                    // Check if MoveMeta list is not empty and the first item has a MoveMetaAilment
                    if (move.MoveMeta != null && move.MoveMeta.Count > 0 && move.MoveMeta[0].MoveMetaAilment != null)
                    {
                        Debug.Log($"- {move.Name} (Ailment: {move.MoveMeta[0].MoveMetaAilment.Name})");
                    }
                    else
                    {
                        Debug.Log($"- {move.Name} (Ailment information not available)");
                    }
                }
            }
            else
            {
                Debug.LogWarning("No MovesWithAilment Data received");
            }
        } 
        /*
        void SendMovesLearnedByPokemonQuery()
        {
            var query = new QueryBuilder()
                .Operation("MovesLearnedByPokemon")
                .Variable("pokemonName", "vulpix", "String!")
                .BeginObject("pokemon_v2_pokemon(where: {name: {_eq: $pokemonName}})")
                    .Field("name")
                    .BeginObject("pokemon_v2_pokemonmoves")
                        .BeginObject("pokemon_v2_move")
                            .Field("name")
                            .BeginObject("pokemon_v2_type")
                                .Field("name")
                            .EndObject()
                        .EndObject()
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending MovesLearnedByPokemon query: {query.query}");
            graphQLClient.SendQuery<MovesLearnedByPokemonResponse>(query, HandleMovesLearnedByPokemonResponse);
        }

        void HandleMovesLearnedByPokemonResponse(GraphQLResponse<MovesLearnedByPokemonResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"MovesLearnedByPokemon query failed: {error.Message}");
                return;
            }

            if (response.Data?.pokemon_v2_pokemon != null && response.Data.pokemon_v2_pokemon.Count > 0)
            {
                var pokemon = response.Data.pokemon_v2_pokemon[0];
                Debug.Log($"Moves learned by {pokemon.name}:");
                foreach (var moveInfo in pokemon.pokemon_v2_pokemonmoves)
                {
                    Debug.Log($"- {moveInfo.pokemon_v2_move.name} (Type: {moveInfo.pokemon_v2_move.pokemon_v2_type.Name})");
                }
            }
            else
            {
                Debug.LogWarning("No MovesLearnedByPokemon Data received");
            }
        }
        */
        void SendMovesWithAccuracyAndTypeQuery()
        {
            var query = new QueryBuilder()
                .Operation("MovesWithAccuracyAndType")
                .Variable("accuracy", 90, "Int!")
                .Variable("type", "water", "String!")
                .BeginObject("pokemon_v2_move(where: {accuracy: {_eq: $accuracy}, pokemon_v2_type: {name: {_eq: $type}}})")
                    .Field("name")
                    .Field("accuracy")
                    .Field("power")
                    .Field("pp")
                    .BeginObject("pokemon_v2_type")
                        .Field("name")
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending MovesWithAccuracyAndType query: {query.query}");
            graphQLClient.SendQuery<MovesWithAccuracyAndTypeResponse>(query, HandleMovesWithAccuracyAndTypeResponse);
        }

        void HandleMovesWithAccuracyAndTypeResponse(GraphQLResponse<MovesWithAccuracyAndTypeResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"MovesWithAccuracyAndType query failed: {error.Message}");
                return;
            }

            if (response.Data?.pokemon_v2_move != null)
            {
                Debug.Log($"Moves with 90 accuracy of type water:");
                foreach (var move in response.Data.pokemon_v2_move)
                {
                    Debug.Log($"- {move.name} (Power: {move.power}, PP: {move.pp})");
                }
            }
            else
            {
                Debug.LogWarning("No MovesWithAccuracyAndType Data received");
            }
        }

        void SendDualTypePokemonInGenQuery()
        {
            var query = new QueryBuilder()
                .Operation("DualTypePokemonInGen")
                .Variable("gen", 1, "Int!")
                .Variable("type1", "fire", "String!")
                .Variable("type2", "flying", "String!")
                .BeginObject("pokemon_v2_pokemon(where: {pokemon_v2_pokemonspecy: {generation_id: {_eq: $gen}}, _and: [{pokemon_v2_pokemontypes: {pokemon_v2_type: {name: {_eq: $type1}}}}, {pokemon_v2_pokemontypes: {pokemon_v2_type: {name: {_eq: $type2}}}}]})")
                    .Field("name")
                    .BeginObject("pokemon_v2_pokemontypes")
                        .BeginObject("pokemon_v2_type")
                            .Field("name")
                        .EndObject()
                    .EndObject()
                    .BeginObject("pokemon_v2_pokemonspecy")
                        .Field("generation_id")
                    .EndObject()
                .EndObject()
                .Build();

            Debug.Log($"Sending DualTypePokemonInGen query: {query.query}");
            graphQLClient.SendQuery<DualTypePokemonInGenResponse>(query, HandleDualTypePokemonInGenResponse);
        }

        void HandleDualTypePokemonInGenResponse(GraphQLResponse<DualTypePokemonInGenResponse> response, Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"DualTypePokemonInGen query failed: {error.Message}");
                return;
            }

            if (response.Data?.pokemon_v2_pokemon != null)
            {
                Debug.Log($"Dual-type (Fire/Flying) Pokémon in Gen 1:");
                foreach (var pokemon in response.Data.pokemon_v2_pokemon)
                {
                    Debug.Log($"- {pokemon.name}");
                }
            }
            else
            {
                Debug.LogWarning("No DualTypePokemonInGen Data received");
            }
        }
    }



    [Serializable]
    public class PokemonSpecies
    {
        [JsonProperty("aggregate")]
        public Aggregate aggregate;
    }


    [Serializable]
    public class PokemonDetailResponse
    {
        [JsonProperty("pokemon")]
        public List<PokemonDetail> pokemon;
    }

    [Serializable]
    public class PokemonDetail
    {
        [JsonProperty("id")]
        public int id;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("types")]
        public List<PokemonTypeInfo> types;
    }

    [Serializable]
    public class PokemonTypeInfo
    {
        [JsonProperty("type")]
        public PokemonType type;
    }


    [Serializable]
    public class PokemonLocationResponse
    {
        [JsonProperty("location")]
        public List<LocationArea> Location { get; set; }
    }

    [Serializable]
    public class LocationArea
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pokemon_v2_encounters")]
        public List<Encounter> PokemonV2Encounters { get; set; }
    }

    [Serializable]
    public class Encounter
    {
        [JsonProperty("pokemon_v2_pokemon")]
        public Pokemon PokemonV2Pokemon { get; set; }
    }

    [Serializable]
    public class Pokemon
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [Serializable]
    public class PokemonPokedexEntriesResponse
    {
        [JsonProperty("pokemon_v2_pokemonspeciesflavortext")]
        public List<PokedexEntry> PokemonV2PokemonspeciesflavorText { get; set; }
    }

    [Serializable]
    public class PokedexEntry
    {
        [JsonProperty("flavor_text")]
        public string FlavorText { get; set; }

        [JsonProperty("pokemon_species_id")]
        public int PokemonSpeciesId { get; set; }

        [JsonProperty("pokemon_v2_version")]
        public Version PokemonV2Version { get; set; }

        [JsonProperty("pokemon_v2_language")]
        public Language PokemonV2Language { get; set; }
    }

    [Serializable]
    public class Version
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }

    [Serializable]
    public class Language
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    /// SECOND TEST 

    [Serializable]
    public class PokemonsByTypeAndGenResponse
    {
        public List<PokemonTypeGen> pokemon_v2_pokemon { get; set; }
    }

    [Serializable]
    public class PokemonTypeGen
    {
        public string name { get; set; }
        public PokemonSpecyGen pokemon_v2_pokemonspecy { get; set; }
        public List<PokemonTypeInfo> pokemon_v2_pokemontypes { get; set; }
    }

    [Serializable]
    public class PokemonSpecyGen
    {
        public int generation_id { get; set; }
    }

    [Serializable]
    public class PokemonHiddenAbilityResponse
    {
        public List<PokemonAbility> pokemon_v2_pokemonability { get; set; }
    }

    [Serializable]
    public class PokemonAbility
    {
        public Ability pokemon_v2_ability { get; set; }
        public PokemonName pokemon_v2_pokemon { get; set; }
    }

    [Serializable]
    public class Ability
    {
        public string name { get; set; }
    }
    
    [Serializable]
    public class PokemonName
    {
        public string name { get; set; }
    }

    [Serializable]
    public class PokemonType
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    /*
    [Serializable]
    public class PokemonMove
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pokemon_v2_type")]
        public PokemonType Type { get; set; }
    }
    */

    [Serializable]
    public class MovesWithAilmentResponse
    {
        [JsonProperty("pokemon_v2_move")]
        public List<Move> Moves { get; set; }

        [Serializable]
        public class Move
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("pokemon_v2_movemeta")]
            public List<MoveMeta> MoveMeta { get; set; }
        }

        [Serializable]
        public class MoveMeta
        {
            [JsonProperty("pokemon_v2_movemetaailment")]
            public MoveMetaAilment MoveMetaAilment { get; set; }
        }

        [Serializable]
        public class MoveMetaAilment
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
    /*
    [Serializable]
    public class MovesLearnedByPokemonResponse
    {
        public List<Pokemon> pokemon_v2_pokemon { get; set; }

        [Serializable]
        public class Pokemon
        {
            public string name { get; set; }
            public List<PokemonMove> pokemon_v2_pokemonmoves { get; set; }
        }

        [Serializable]
        public class PokemonMove
        {
            public Move pokemon_v2_move { get; set; }
        }

        [Serializable]
        public class Move
        {
            public string name { get; set; }
            public PokemonType pokemon_v2_type { get; set; }
        }
    }
    */
    [Serializable]
    public class MovesWithAccuracyAndTypeResponse
    {
        public List<Move> pokemon_v2_move { get; set; }

        [Serializable]
        public class Move
        {
            public string name { get; set; }
            public int accuracy { get; set; }
            public int? power { get; set; }
            public int pp { get; set; }
            public PokemonType pokemon_v2_type { get; set; }
        }
    }

    [Serializable]
    public class DualTypePokemonInGenResponse
    {
        public List<Pokemon> pokemon_v2_pokemon { get; set; }

        [Serializable]
        public class Pokemon
        {
            public string name { get; set; }
            public List<PokemonTypeInfo> pokemon_v2_pokemontypes { get; set; }
            public PokemonSpecy pokemon_v2_pokemonspecy { get; set; }
        }

        [Serializable]
        public class PokemonTypeInfo
        {
            public PokemonType pokemon_v2_type { get; set; }
        }

        [Serializable]
        public class PokemonSpecy
        {
            public int generation_id { get; set; }
        }
    }

    [Serializable]
    public class PokemonSpeciesBase
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [Serializable]
    public class PokemonSpeciesAggregate : PokemonSpeciesBase
    {
        [JsonProperty("aggregate")]
        public Aggregate Aggregate { get; set; }
    }

    [Serializable]
    public class PokemonSpeciesDetailed : PokemonSpeciesBase
    {
        [JsonProperty("pokemon_v2_generation")]
        public Generation Generation { get; set; }
    }

    [Serializable]
    public class Aggregate
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    [Serializable]
    public class Generation
    {
        [JsonProperty("pokemon_v2_region")]
        public Region Region { get; set; }
    }

    [Serializable]
    public class Region
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    // Update the PokemonGenerationResponse to use the new PokemonSpeciesAggregate
    [Serializable]
    public class PokemonGenerationResponse
    {
        [JsonProperty("generations")]
        public List<GenerationWithAggregate> Generations { get; set; }
    }

    [Serializable]
    public class GenerationWithAggregate
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pokemon_species")]
        public PokemonSpeciesAggregate PokemonSpecies { get; set; }
    }

    // Update the PokemonNativeRegionResponse to use the new PokemonSpeciesDetailed
    [Serializable]
    public class PokemonNativeRegionResponse
    {
        [JsonProperty("pokemon_v2_pokemonspecies")]
        public List<PokemonSpeciesDetailed> PokemonV2PokemonSpecies { get; set; }
    }

}