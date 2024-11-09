using Castrimaris.IO.GoogleDirections;
using Castrimaris.IO.OpenAI.Embeddings;
using OpenAI.Embeddings;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO.OpenAI {

    [RequireComponent(typeof(OpenAIApi))]
    public class DirectionsEmbeddings : MonoBehaviour {

        [Header("Parameteres")]
        [SerializeField] private DirectionsEmbeddingsContainer embeddings;

        [Header("Events")]
        [Tooltip("Event called when the input for the completion is available.")]
        [SerializeField] private UnityEvent<string> onCompletionEmbeddingAvailable = new UnityEvent<string>();
        [Tooltip("Event called when the embedding with the directions is available")]
        [SerializeField] private UnityEvent<DirectionsContainer> onEmbeddingAvailable = new UnityEvent<DirectionsContainer>();

        private OpenAIApi api;

        public UnityEvent<string> OnClosestEmbeddingFound => onCompletionEmbeddingAvailable;

        public async void FindClosestMatch(string input) {
            var request = new EmbeddingsRequest(input);
            var response = await api.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(request);
            var embedding = await embeddings.GetMostRelevantEmbedding(response.Data.First().Embedding.ToArray());
            var instructions = embedding.Instructions;
            var inputWithEmbedding = //TODO make it language agnostic (e.g. italian, english, french, etc.)
                $"Di seguito vengono elencate delle istruzioni;" +
                $"usando le istruzioni disponibili qui di seguito, rispondi alla domanda. " +
                $"Usa un tono colloquiale nel fornire le istruzioni." +
                $"\nIstruzioni: {instructions}" +
                $"\nDomanda: {input}" +
                $"\nUna volta terminato di fornire le istruzioni dici letteralmente la seguente frase senza rielaborarla: " +
                $"\nè ora possibile salire sulla piattaforma alla mia sinistra per visualizzare i punti salienti del percorso."; 
            onCompletionEmbeddingAvailable.Invoke(inputWithEmbedding);
            onEmbeddingAvailable.Invoke(embedding);
        }

        private void Awake() {
            api = GetComponent<OpenAIApi>();
        }

    }

}