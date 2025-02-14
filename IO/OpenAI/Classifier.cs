using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Role = OpenAI.Role;
using OpenAI.Chat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading;

namespace Castrimaris.IO.OpenAI {

    /// <summary>
    /// Categorizes a text input based on the available categories, then invokes the relative callback
    /// </summary>
    [RequireComponent(typeof(OpenAIApi))]
    public class Classifier : MonoBehaviour {

        #region Private Variables

        [Header("Parameters")]
        [Tooltip("General category to use when there's no categorization available")]
        [SerializeField] private string fallbackCategory = "Altro";
        [Tooltip("The categories that the AI will use to try and categorize the input.")]
        [SerializeField] private List<string> categories = new List<string>();
        [Tooltip("The model used to categorize the input")]
        [SerializeField] private Models model = Models.GPT3_5_Turbo;

        [Header("Events")]
        [Tooltip("Event called when the categorization is done successfully with one of the categories.")]
        [SerializeField] private UnityEvent<string>[] onCategorized;
        [Tooltip("Event called when the categorization is unsuccessful with the indicated categories")]
        [SerializeField] private UnityEvent<string> onUncategorized;

        private OpenAIApi api;
        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Properties

        public UnityEvent<string>[] OnCategorized => onCategorized;
        public UnityEvent<string> OnUncategorized => onUncategorized;

        #endregion

        #region Public Methods

        /// <summary>
        /// Categorizes an input based on the available categories.
        /// </summary>
        /// <param name="input">The input to categorize</param>
        public async void Categorize(string input) {
            Log.D($"Trying to categorize input '{input}'");
            var messages = BuildMessages(input);
            var chatRequest = new ChatRequest(messages: messages, model: model.GetStringValue());
            var index = await RequestCategorization(chatRequest);
            if (index < 0 || index == categories.Count) {
                Log.D($"No categorization found for {input}. Proceding on uncategorization.");
                onUncategorized.Invoke(input);
            } else {
                Log.D($"Categorization found for {input}. Proceding on category {categories[index]}.");
                onCategorized[index].Invoke(input);
            }
        }

        public void Abort() {
            cancellationTokenSource.Cancel();
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            api = GetComponent<OpenAIApi>();

            if (onCategorized.Length != categories.Count) {
                throw new System.IndexOutOfRangeException($"Count of {nameof(categories)} and {nameof(onCategorized)} do not match. Please ensure that they are the same count at all times.");
            }
        }

        #endregion

        #region Private Methods

        private async Task<int> RequestCategorization(ChatRequest request) {
            cancellationTokenSource = new CancellationTokenSource();
            var response = await api.Client.ChatEndpoint.GetCompletionAsync(request, cancellationTokenSource.Token);
            if (cancellationTokenSource.IsCancellationRequested) return -1;
            var result = response.FirstChoice.Message.Content.ToString();
            Log.D($"Received response from categorization: {result}");
            if (!categories.Contains(result))
                return -1;
            var index = categories.IndexOf(result);
            return index;
        }

        private List<Message> BuildMessages(string input) {
            var list = new List<Message>();
            list.Add(new Message(role: Role.System, "Sei un assistente AI"));

            //we add an additional field to fallback to
            var categoriesWithFallback = new List<string>(categories);
            categoriesWithFallback.Add(fallbackCategory);

            var message = new Message(
                role: Role.User,
                content: $"Usando una ed una sola delle seguenti categorie:\n" +
                $"- {string.Join("\n - ", categoriesWithFallback)}" +
                $"indica la categoria, e solo la categoria, senza altre parole, per la seguente frase:" +
                $"{input}"
                );
            list.Add(message);

            return list;
        }


        #endregion
    }

}