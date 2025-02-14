using Castrimaris.Attributes;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.Contracts;
using Castrimaris.ScriptableObjects;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Threads;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Utilities.WebRequestRest.Interfaces;

namespace Castrimaris.IO.OpenAI {

    [RequireComponent(typeof(OpenAIApi))]
    public class Assistant : MonoBehaviour, IAssistant {

        #region Private Variables

        [Header("Parameters")]
        [Tooltip("Wheter to use the streaming of results, rather than just the full result.\nEnabling this uses the partial events.")]
        [SerializeField] private bool usePartialResults = false;

        [Header("References")]
        [SerializeField] private APIKey assistantKey;

        [Header("ReadOnly Parameters")]
        [SerializeField, ReadOnly] private bool isThinking = false;

        [Header("Events")]
        [Tooltip("List of events fired when a thread request is about to be sent")]
        [SerializeField] private UnityEvent onRequest = new UnityEvent();
        [Tooltip("List of events fired when the complete thread response is received.")]
        [SerializeField] private UnityEvent<string> onResponse = new UnityEvent<string>();
        [Tooltip("List of events fired when a token of the thread response is received.")]
        [SerializeField] private UnityEvent<string> onPartialResponse = new UnityEvent<string>();

        private OpenAIApi apis;
        private ThreadResponse thread;
        private AssistantResponse assistant;
        private volatile int partialResultIndex = 0;
        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Properties

        public bool IsThinking => isThinking;
        public UnityEvent<string> OnChatResponse => onResponse;
        public UnityEvent<string> OnPartialChatResponse => onPartialResponse;

        #endregion

        #region Public Methods

        public void Ask(string Text) {
            cancellationTokenSource = new CancellationTokenSource();
            isThinking = true;
            onRequest.Invoke();
            var message = new Message(Text, Role.User);
            SendThreadMessage(message);
        }

        public void Abort() {
            cancellationTokenSource.Cancel();
        }

        #endregion

        #region Unity Overrides

        private async void Awake() {
            apis = GetComponent<OpenAIApi>();

            if (assistantKey == null) throw new ReferenceMissingException(nameof(assistantKey));

            thread = await apis.Client.ThreadsEndpoint.CreateThreadAsync();
            assistant = await apis.Client.AssistantsEndpoint.RetrieveAssistantAsync(assistantKey.Key);
        }

        #endregion

        #region Private Methods

        private async void SendThreadMessage(Message message) {
            var messageCreationResponse = await apis.Client.ThreadsEndpoint.CreateMessageAsync(thread.Id, message);
            if (cancellationTokenSource.IsCancellationRequested) return;
            var threadRun = await thread.CreateRunAsync(assistant, ThreadRunHandler);
            isThinking = false;
            partialResultIndex = 0;
        }

        private async Task ThreadRunHandler(IServerSentEvent serverSentEvent) {
            var json = serverSentEvent.ToJsonString();
            Log.D($"Jsonized content received: {json}");

            switch (serverSentEvent.Object) {
                case "thread.run":
                    var run = JsonConvert.DeserializeObject<RunResponse>(json);
                    Log.D($"Received run response");
                    break;

                case "thread.run.step":
                    var runStep = JsonConvert.DeserializeObject<RunStepResponse>(json);
                    Log.D($"Received run step response");
                    break;

                case "thread.message":
                    var messageResponse = JsonConvert.DeserializeObject<MessageResponse>(json);
                    Log.D($"Received message response ${string.Join("",messageResponse.Content)}");
                    //TODO understand if message response can be called as last resort
                    break;

                case "thread.message.delta":
                    if (!usePartialResults)
                        return;

                    var messageDelta = JsonConvert.DeserializeObject<MessageDelta>(json);
                    var messageDeltaContent = messageDelta.PrintContent();
                    var delta = messageDeltaContent.Substring(partialResultIndex);
                    partialResultIndex = messageDeltaContent.Length;

                    Log.D($"Received message delta {delta}");
                    onPartialResponse.Invoke(delta);
                    break;

                default:
                    Log.E($"No case recognized for {serverSentEvent.Object}! If debug logging is enabled check the previous message for jsonized content.");
                    break;
            }
            await Task.CompletedTask;
        }

        #endregion
    }
}
