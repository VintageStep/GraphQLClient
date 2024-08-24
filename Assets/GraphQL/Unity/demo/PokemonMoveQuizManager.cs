using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Unity;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Newtonsoft.Json;

namespace GraphQL.Unity.Demo
{

    public class PokemonMoveQuizManager : MonoBehaviour
    {
        [SerializeField] private GraphQLClientBehaviour graphQLClient;
        [SerializeField] private Text instructionsText;
        [SerializeField] private InputField userInputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Text timerText;
        [SerializeField] private Text scoreText;
        [SerializeField] private ScrollRect movesScrollRect;
        [SerializeField] private Text enteredMoveTextPrefab;
        [SerializeField] private List<string> pokemonList;

        [SerializeField] private float moveRevealDelay = 0.1f;
        [SerializeField] private VerticalLayoutGroup movesLayoutGroup;

        private Dictionary<string, HashSet<string>> movesByType;
        private string currentPokemon;
        private string currentType;
        private HashSet<string> enteredMoves;
        private float timeRemaining = 60f;
        private bool isGameActive = false;
        private string chosenPokemon = "";
        private float score = 0;

        private TaskCompletionSource<bool> dataLoadedTcs;

        private void Start()
        {
            if (submitButton == null || userInputField == null || startButton == null)
            {
                Debug.LogError("Some UI components are not assigned. Please check the Inspector.");
                return;
            }

            submitButton.interactable = false;
            userInputField.interactable = false;
            submitButton.onClick.AddListener(SubmitMove);
            startButton.onClick.AddListener(StartGame);
            userInputField.onEndEdit.AddListener(OnInputFieldSubmit);

            StartCoroutine(LateStart());

        }

        // Attempt to resolve viewport awake() warning
        private IEnumerator LateStart()
        {
            yield return null;
            UpdateLayoutCoroutine(); // UNT0012 advice to use a courroutine but that's just more nesting, which I don't think is wise
        }

        private void RandomPokemon()
        {
            chosenPokemon = pokemonList[Random.Range(0, pokemonList.Count)];
        }

        private async void StartGame()
        {
            Debug.Log("StartGame called");
            if (isGameActive)
            {
                Debug.Log("Game is already active, ignoring start request");
                return;
            }

            await StartGameAsync();
        }

        private async Task StartGameAsync()
        {
            Debug.Log("StartGameAsync called");
            ResetGameState();
            isGameActive = true;
            submitButton.interactable = false;
            userInputField.interactable = false;

            instructionsText.text = "Loading Pokémon data...";

            RandomPokemon();
            bool dataLoaded = await FetchPokemonDataAsync();

            if (!dataLoaded || movesByType == null || movesByType.Count == 0)
            {
                Debug.LogError("Failed to load Pokemon data");
                instructionsText.text = "Failed to load data. Please try again.";
                ResetGameState();
                return;
            }

            submitButton.interactable = true;
            userInputField.interactable = true;
            currentType = movesByType.Keys.ElementAt(Random.Range(0, movesByType.Count));
            enteredMoves = new HashSet<string>();

            UpdateInstructions();
            UpdateScore();
            ClearEnteredMoves();

            Debug.Log($"Game started with Pokemon: {currentPokemon}, Type: {currentType}");
        }

        private async Task<bool> FetchPokemonDataAsync()
        {
            var query = new QueryBuilder()
                .Operation("MovesLearnedByPokemon")
                .Variable("pokemonName", chosenPokemon, "String!")
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

            var query2 = new QueryBuilder()
                .Operation("GetPokemon")
                .Variable("name", name, "String!")
                .BeginObject("pokemon(name: $name)")
                    .Field("id")
                    .Field("name")
                    .Field("weight")
                .EndObject()
                .Build();

            Debug.Log($"valid?: {query2.query}");

            dataLoadedTcs = new TaskCompletionSource<bool>();
            graphQLClient.SendQuery<MovesLearnedByPokemonResponse>(query, OnPokemonDataReceived);
            return await dataLoadedTcs.Task;
        }

        private void OnPokemonDataReceived(GraphQLResponse<MovesLearnedByPokemonResponse> response, System.Exception error)
        {
            if (error != null)
            {
                Debug.LogError($"Error fetching Pokemon data: {error.Message}");
                dataLoadedTcs.SetResult(false);
                return;
            }

            if (response.Data?.pokemon_v2_pokemon != null && response.Data.pokemon_v2_pokemon.Count > 0)
            {
                var pokemon = response.Data.pokemon_v2_pokemon[0];
                currentPokemon = pokemon.name;
                movesByType = new Dictionary<string, HashSet<string>>();

                foreach (var moveInfo in pokemon.pokemon_v2_pokemonmoves)
                {
                    var moveType = moveInfo.pokemon_v2_move.pokemon_v2_type.Name;
                    var moveName = moveInfo.pokemon_v2_move.name.Replace("-", " ").ToLower();

                    if (!movesByType.ContainsKey(moveType))
                    {
                        movesByType[moveType] = new HashSet<string>();
                    }
                    movesByType[moveType].Add(moveName);
                }

                Debug.Log($"Data loaded for Pokemon: {currentPokemon}, Move types: {string.Join(", ", movesByType.Keys)}");
                dataLoadedTcs.SetResult(true);
            }
            else
            {
                Debug.LogWarning("No Pokemon data received");
                dataLoadedTcs.SetResult(false);
            }
        }

        private void UpdateInstructions()
        {
            instructionsText.text = $"Name the {currentType}-type moves that {currentPokemon} can learn:";
        }

        private void UpdateScore()
        {
            int totalMoves = movesByType[currentType].Count;
            int correctMoves = enteredMoves.Intersect(movesByType[currentType], System.StringComparer.OrdinalIgnoreCase).Count();
            score = (float)correctMoves / totalMoves * 100f;
            scoreText.text = $"Score: {score:F1}%";
            Debug.Log("Total moves of type " + currentType + ": " + totalMoves);
        }

        private void SubmitMove()
        {
            if (isGameActive)
            {
                string move = userInputField.text.Trim().ToLower().Replace("-", " ");
                if (!string.IsNullOrEmpty(move) && !enteredMoves.Contains(move))
                {
                    enteredMoves.Add(move);
                    DisplayEnteredMove(move);
                    UpdateScore();
                }
                userInputField.text = "";
                userInputField.ActivateInputField();
            }
        }


        private void OnInputFieldSubmit(string value)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitMove();
            }
        }

        private void DisplayEnteredMove(string move)
        {
            Text moveText = Instantiate(enteredMoveTextPrefab, movesScrollRect.content);
            moveText.text = move;
            moveText.color = movesByType[currentType].Contains(move) ? Color.green : Color.red;


            StartCoroutine(UpdateLayoutCoroutine());
        }

        private void ClearEnteredMoves()
        {
            foreach (Transform child in movesScrollRect.content)
            {
                Destroy(child.gameObject);
            }

            StartCoroutine(UpdateLayoutCoroutine());
        }

        private void Update()
        {
            if (isGameActive)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0 || score >= 100)
                {
                    EndGame();
                }
                else
                {
                    timerText.text = $"Time: {timeRemaining:F1}s";
                }
            }
        }

        private void ResetGameState()
        {
            Debug.Log("Resetting game state");
            isGameActive = false;
            dataLoadedTcs = null;
            movesByType = null;
            currentPokemon = null;
            currentType = null;
            enteredMoves = null;
            timeRemaining = 60f;
            score = 0f;
            chosenPokemon = "";
            Debug.Log("Game state reset complete");
        }

        private void EndGame()
        {
            Debug.Log("Ending game");
            isGameActive = false;
            submitButton.interactable = false;
            userInputField.interactable = false;
            instructionsText.text = "Game Over!";

            if (movesByType != null && currentType != null)
            {
                string rightMoves = string.Join(", ", movesByType[currentType]);
                Debug.Log("Correct Moves: " + rightMoves);
                StartCoroutine(DisplayCorrectMovesAndResetGame());
            }
            else
            {
                Debug.LogWarning("movesByType or currentType is null at game end");
                ResetGameState();
            }
        }


        private IEnumerator DisplayCorrectMovesAndResetGame()
        {
            yield return StartCoroutine(DisplayCorrectMovesCoroutine());
            ResetGameState();
        }


        private IEnumerator DisplayCorrectMovesCoroutine()
        {
            if (movesByType == null || currentType == null)
            {
                Debug.LogError("movesByType or currentType is null in DisplayCorrectMovesCoroutine");
                yield break;
            }

            if (movesScrollRect == null || movesScrollRect.content == null)
            {
                Debug.LogError("movesScrollRect or its content is null");
                yield break;
            }

            if (enteredMoveTextPrefab == null)
            {
                Debug.LogError("enteredMoveTextPrefab is not assigned");
                yield break;
            }

            Text headerText = Instantiate(enteredMoveTextPrefab, movesScrollRect.content);
            if (headerText != null)
            {
                headerText.text = "Correct Moves:";
                headerText.color = Color.yellow;
            }
            else
            {
                Debug.LogError("Failed to instantiate headerText");
                yield break;
            }

            yield return StartCoroutine(UpdateLayoutCoroutine());

            foreach (string move in movesByType[currentType].OrderBy(m => m))
            {
                Text moveText = Instantiate(enteredMoveTextPrefab, movesScrollRect.content);
                if (moveText != null)
                {
                    moveText.text = move;
                    moveText.color = Color.yellow;
                }
                else
                {
                    Debug.LogError($"Failed to instantiate moveText for move: {move}");
                    continue;
                }

                yield return StartCoroutine(UpdateLayoutCoroutine());
                yield return new WaitForSeconds(moveRevealDelay);
            }
        }


        private IEnumerator UpdateLayoutCoroutine()
        {
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(movesScrollRect.content as RectTransform);
            Canvas.ForceUpdateCanvases();
            AdjustContentSize();
            movesScrollRect.verticalNormalizedPosition = 0f;
        }

        private void AdjustContentSize()
        {
            float totalHeight = 0;
            foreach (RectTransform child in movesScrollRect.content)
            {
                totalHeight += child.rect.height + movesLayoutGroup.spacing;
            }
            totalHeight += movesLayoutGroup.padding.top + movesLayoutGroup.padding.bottom;

            RectTransform contentRectTransform = movesScrollRect.content as RectTransform;
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);
        }

    }

    [System.Serializable]
    public class MovesLearnedByPokemonResponse
    {
        public List<Pokemon> pokemon_v2_pokemon { get; set; }

        [System.Serializable]
        public class Pokemon
        {
            public string name { get; set; }
            public List<PokemonMove> pokemon_v2_pokemonmoves { get; set; }
        }

        [System.Serializable]
        public class PokemonMove
        {
            public Move pokemon_v2_move { get; set; }
        }

        [System.Serializable]
        public class Move
        {
            public string name { get; set; }
            public PokemonType pokemon_v2_type { get; set; }
        }
    }

    [System.Serializable]
    public class PokemonType
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}