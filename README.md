# Unity GraphQL Client

A pre-release Unity package that enables seamless integration with GraphQL endpoints. This client offers a robust QueryBuilder for easy construction of queries, mutations, and subscriptions, along with runtime caching and straightforward response parsing.

## Features

- **QueryBuilder**: Intuitive API for constructing GraphQL operations
- **Real-time data fetching**: Efficient communication with GraphQL endpoints
- **Runtime caching**: Improved performance through local data storage
- **Easy response parsing**: Utilize Newtonsoft.Json for simple object conversion
- **Unity integration**: Designed specifically for use within Unity projects

## Technologies Used

- Unity 2022.3 (LTS)
- C# 7.3+
- [GraphQL](https://graphql.org/)
- [Newtonsoft.Json for Unity](https://github.com/jilleJr/Newtonsoft.Json-for-Unity)

## Installation

1. Open or create your Unity project (2022.3 LTS or later).
2. Download the custom package from the [Releases](https://github.com/YourUsername/UnityGraphQLClient/releases) section.
3. Import the package into your Unity project (Assets > Import Package > Custom Package).
4. If the Newtonsoft.Json dependency is missing, follow the instructions [here](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM).

## Configuration and Use

*For this example, once again, we will use the PokeAPI as reference*

1. In your desired Unity scene, create an empty GameObject and name it 'GraphQLClient'.
2. Add the `GraphQLClientBehaviour` script to the 'GraphQLClient' GameObject.
3. Set the GraphQL endpoint URL and authentication token (if required) in the inspector.
4. In your game logic script, create a query using the QueryBuilder:

```csharp
var query = new QueryBuilder()
    .Operation("GetPokemon")
    .BeginObject("pokemon_v2_pokemon")
        .Field("id")
        .Field("name")
    .EndObject()
    .Build();
```

5. Send the query through the client:

```csharp
graphQLClient.SendQuery<PokemonResponse>(query, HandleResponse);
```

6. Process the results in your callback method:

```csharp
private void HandleResponse(GraphQLResponse<PokemonResponse> response, Exception error)
{
    if (error != null)
    {
        Debug.LogError($"Query failed: {error.Message}");
        return;
    }
    
    // Process the response data
    Debug.Log($"Received Pokemon: {response.Data.pokemon.name}");
}
```

## Getting Started

Here's a basic example of how to use the Unity GraphQL Client in your project:

```csharp
using UnityEngine;
using GraphQL.Unity.Core;

public class PokemonFetcher : MonoBehaviour
{
    public GraphQLClientBehaviour graphQLClient;

    private void Start()
    {
        FetchPokemon("pikachu");
    }

    private void FetchPokemon(string pokemonName)
    {
        var query = new QueryBuilder()
            .Operation("GetPokemon")
            .Variable("pokemonName", pokemonName, "String!")
            .BeginObject("pokemon_v2_pokemon(where: {name: {_eq: $pokemonName}})")
                .Field("id")
                .Field("name")
                .Field("weight")
            .EndObject()
            .Build();

        graphQLClient.SendQuery<PokemonResponse>(query, HandlePokemonResponse);
    }

    private void HandlePokemonResponse(GraphQLResponse<PokemonResponse> response, Exception error)
    {
        if (error != null)
        {
            Debug.LogError($"Failed to fetch Pokemon: {error.Message}");
            return;
        }

        var pokemon = response.Data.pokemon;
        Debug.Log($"Fetched Pokemon: {pokemon.name} (ID: {pokemon.id}, Weight: {pokemon.weight})");
    }
}

[System.Serializable]
public class PokemonResponse
{
    public Pokemon pokemon;
}

[System.Serializable]
public class Pokemon
{
    public string id;
    public string name;
    public int weight;
}
```

## Current Limitations

This pre-release version has several limitations that will be addressed in future updates:

- **QueryBuilder input validation**: Robust error checking and validation has to be implemented to ensure query integrity.
- **Advanced filtering options**: The ability to add complex filters as separate entities in the QueryBuilder.
- **Schema introspection and caching**: Future versions will support schema introspection, enabling code autocompletion for the QueryBuilder and reducing reliance on string inputs.
- **Schema visualization**: A graphical representation of the GraphQL schema is planned for easier navigation and query construction.
- **Unity Editor integration**: A dedicated tool within the Unity Editor for managing GraphQL operations is planned for development.

## API Reference

For more information on GraphQL, refer to the official GraphQL documentation.
This project uses the PokeAPI GraphQL Beta to showcase the package's capabilities.

##Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

##License

This project is licensed under the MIT License - see the LICENSE.md file for details.


##Acknowledgments

- Thanks to PokeAPI for providing the Pokémon data and their GraphQL console as a learning tool.
- Thanks to the GraphQL-dotnet organization for providing documentation and usage examples.

Contact
Project Link: https://github.com/VintageStep/GraphQLClient
For issues, feature requests, or general inquiries, please open an issue on the GitHub repository.
