#if OPENAI

using Castrimaris.Attributes;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.Contracts;
using Castrimaris.IO.ScriptableObjects;
using OpenAI;
using OpenAI.Chat;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO.OpenAI {

    /// <summary>
    /// An access point for the stateless chat completions of OpenAI's ChatGPT models
    /// </summary>
    [RequireComponent(typeof(OpenAIApi))]
    public class Completion : MonoBehaviour, IAssistant {

        #region Private Variables

        [Header("Parameters")]
        [SerializeField] private Models chatRequestModel;
        [SerializeField] private bool usePartialResults = false;
        [SerializeField] private bool useTools = false;

        [Header("References")]
        [SerializeField] private ChatGPTConfiguration configuration;

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private bool isThinking = false;

        [Header("Events")]
        [Tooltip("List of events fired when the chat request is sent.")]
        [SerializeField] private UnityEvent onChatRequest = new UnityEvent();
        [Tooltip("List of events fired when the complete chat response is received. The argument is the content of the chat.")]
        [SerializeField] private UnityEvent<string> onChatResponse = new UnityEvent<string>();
        [Tooltip("List of events fired when received a token of the chat response. The argument is the token itself.")]
        [SerializeField] private UnityEvent<string> onPartialChatResponse = new UnityEvent<string>();

        private List<Message> chatHistory = new List<Message>();
        private OpenAIApi apis;
        private List<Tool> tools;
        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Properties

        /// <summary>
        /// Wheter the AI has completed or not its response generation process.
        /// </summary>
        public bool IsThinking => isThinking;

        /// <summary>
        /// List of events fired when the chat request is sent.
        /// </summary>
        public UnityEvent OnChatRequest => onChatRequest;

        /// <summary>
        /// List of events fired when the complete chat response is received. The argument is the content of the chat.
        /// </summary>
        public UnityEvent<string> OnChatResponse => onChatResponse;

        /// <summary>
        /// List of events fired when a part of the chat response is received. The argument is the partial content of the chat.
        /// </summary>
        public UnityEvent<string> OnPartialChatResponse => onPartialChatResponse;

        #endregion

        #region Public Methods

        /// <summary>
        /// Asks something to the AI Assistant
        /// </summary>
        /// <param name="input">What to ask.</param>
        public void Ask(string input) {
            isThinking = true;
            Log.D($"Asking to Completion: {input}");
            chatHistory.Add(new Message(Role.User, input));
            var chatRequest = new ChatRequest(messages: chatHistory, model: chatRequestModel.GetStringValue(), tools: tools);
            SendChatRequest(chatRequest);
        }

        public void Abort() {
            cancellationTokenSource.Cancel();
            isThinking = false;
        }

        #endregion

        #region Unity Overrides

        private void Awake() {
            if (configuration == null) throw new ReferenceMissingException(nameof(configuration));

            apis = GetComponent<OpenAIApi>();

            tools = (useTools) ? GetComponents<ITool>().ToToolsList() : null;

            chatHistory.Clear();
            chatHistory.AddRange(configuration.Messages);
        }

        #endregion

        #region Private Methods

        private async void SendChatRequest(ChatRequest request) {
            Log.D($"Sending request...");
            onChatRequest.Invoke();
            var response = (usePartialResults) ? await apis.Client.ChatEndpoint.StreamCompletionAsync(request, PartialResultHandler, false, cancellationTokenSource.Token) : await apis.Client.ChatEndpoint.GetCompletionAsync(request, cancellationTokenSource.Token);
            if (cancellationTokenSource.IsCancellationRequested ) return;
            var result = response.FirstChoice.Message.Content.ToString();
            chatHistory.Add(response.FirstChoice.Message);
            Log.D($"Received full response: {result}");
            OnChatResponse.Invoke(result);
            isThinking = false;
        }

        private void PartialResultHandler(ChatResponse partialResponse) {
            var partialResult = partialResponse.FirstChoice.Delta.ToString();
            OnPartialChatResponse.Invoke(partialResult);
        }

        #endregion
    }
}

#endif