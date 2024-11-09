using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Castrimaris.IO {

    /// <summary>
    /// Base class for creating OpenAI's embeddings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EmbeddingsBase<T> : ScriptableObject {

        /// <summary>
        /// Wrapper struct for a list of doubles, necessary because otherwise Unity won't serialize correctly of list of lists
        /// </summary>
        [System.Serializable] 
        public struct Weights {
            public List<double> Values;

            public Weights(List<double> Values) => this.Values = Values;
            public Weights(double[] Values) => this.Values = Values.ToList();

            /// <summary>
            /// Shorthand for the extension method <see cref="DoubleExtensions.CosineSimilarity(List{double}, double[], bool)"/>
            /// </summary>
            public double CosineSimilarity(double[] weights, bool shouldBeNormalized = false) => Values.CosineSimilarity(weights, shouldBeNormalized);
        }

        [Header("Parameters")]
        [SerializeField] protected APIKey apiKey;
        [SerializeField] protected string id = ""; //An Id for these embeddings
        [SerializeField, Multiline] protected List<string> texts = new List<string>(); //Texts used to calculate the weights
        [SerializeField] protected List<T> data = new List<T>(); //Additional data relative to the embedding itself
        [SerializeField] protected List<Weights> weights = new List<Weights>();

        public List<T> Data { set => data = value; }
        public int Count { get => texts.Count; }

        /// <summary>
        /// Generates embeddings for this Scriptable Object.
        /// </summary>
        public abstract Task GenerateEmbeddings();

        /// <summary>
        /// Retrieves the most relevant embedding from the available embeddings
        /// </summary>
        /// <param name="Weights"></param>
        /// <returns></returns>
        public virtual async Task<T> GetMostRelevantEmbedding(double[] Weights) {
            if (weights.Count <= 0)
                await GenerateEmbeddings();

            int index = 0;
            double maxSimilarity = weights[0].CosineSimilarity(Weights, false);
            for (int i = 1; i < weights.Count; i++) {
                var weight = weights[i];
                var cosineSimilarity = weight.CosineSimilarity(Weights, false);
                if (cosineSimilarity > maxSimilarity) {
                    index = i;
                    maxSimilarity = cosineSimilarity;
                }
            }

            return Task.FromResult(data[index]).Result;
        }

    }

}