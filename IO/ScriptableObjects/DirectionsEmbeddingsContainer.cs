#if OPENAI

using Castrimaris.Core.Monitoring;
using Castrimaris.IO.GoogleDirections;
using OpenAI;
using OpenAI.Embeddings;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Castrimaris.IO.OpenAI.Embeddings {

    /// <summary>
    /// Embeddings for Google directions.
    /// </summary>
    [CreateAssetMenu(fileName = "Directions Embeddings", menuName = "Castrimaris/ScriptableObjects/Directions Embeddings")]
    public class DirectionsEmbeddingsContainer : EmbeddingsBase<DirectionsContainer> {

        public override async Task GenerateEmbeddings() {
            weights.Clear();

            Log.D($"Generating {data.Count} embeddings...");

            var client = new OpenAIClient(apiKey.Key);

            for (int i = 0; i < data.Count; i++) {
                Log.D($"Generating embedding {i + 1} of {data.Count}...");

                var instructions = data[i].Instructions;
                var embeddingText = texts[i] + "\n" + "Direzioni:\n" + instructions;

                Log.D($"Generating embedding for text: {embeddingText}");
                var embeddingsRequest = new EmbeddingsRequest(embeddingText);
                var response = await client.EmbeddingsEndpoint.CreateEmbeddingAsync(embeddingsRequest);
                var embedding = response.Data.First().Embedding.ToList();
                weights.Add(new Weights(embedding));

                Log.D($"Embedding generated");
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif

            Log.D("Done.");
        }

    }

}

#endif