#if OPENAI

using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.IO.ScriptableObjects;
using Castrimaris.ScriptableObjects;
using UnityEngine.Events;
using UnityEngine;
using Castrimaris.Core.Monitoring;
using System.Threading.Tasks;
using Castrimaris.Core.Extensions;
using Newtonsoft.Json;
using Castrimaris.Network;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Castrimaris.Core.Exceptions;

namespace Castrimaris.IO {

    //TODO must be certain that a ThreadDispatcher exists
    /// <summary>
    /// OpenAI's Realtime APIs implementation through <see cref="ManagedWebSocket"/>.
    /// </summary>
    public class Realtime : SingletonMonoBehaviour<Realtime> {

        #region Private Variables

        [Header("Parameters")]
        [Tooltip("Wheter the service should be initialized in Awake, Start or through external means")]
        [SerializeField] private InitializationTypes initializationType = InitializationTypes.OnAwake;
        [Tooltip("How the model should answer prompts")]
        [SerializeField] private OpenAIRealtimeModes realtimeMode = OpenAIRealtimeModes.Text_And_Audio;
        [ConditionalField(nameof(realtimeMode), OpenAIRealtimeModes.Text_And_Audio, DisablingTypes.Hidden)]
        [Tooltip("Voice of the model")]
        [SerializeField] private OpenAIVoices voice = OpenAIVoices.Alloy;
        [ConditionalField(nameof(realtimeMode), OpenAIRealtimeModes.Text_And_Audio, DisablingTypes.Hidden)]
        [Tooltip("Format of the audio coming from the user")]
        [SerializeField] private OpenAIAudioFormat inputAudioFormat = OpenAIAudioFormat.pcm16;
        [ConditionalField(nameof(realtimeMode), OpenAIRealtimeModes.Text_And_Audio, DisablingTypes.Hidden)]
        [Tooltip("Format of the audio coming from the AI model")]
        [SerializeField] private OpenAIAudioFormat outputAudioFormat = OpenAIAudioFormat.pcm16;
        [ConditionalField(nameof(realtimeMode), OpenAIRealtimeModes.Text_And_Audio, DisablingTypes.Hidden)]
        [Tooltip("Wheter the model itself should check for user's voice activation.")]
        [SerializeField] private OpenAIVAD voiceActivationDetection = OpenAIVAD.None;

        [Header("References")]
        [SerializeField] private APIKey apiKey;
        [SerializeField] private ChatGPTConfiguration configuration;

        [Header("Events")]
        [SerializeField] private UnityEvent<AudioClip> onPartialResponse = new UnityEvent<AudioClip>();
        [SerializeField] private UnityEvent<string> onPartialBase64Response = new UnityEvent<string>();
        [SerializeField] private UnityEvent<AudioClip> onResponse = new UnityEvent<AudioClip>();

        private const string eventIdPrepend = "Castrimaris_rt_";
        private const string realtimeEndpoint = "wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01";

        private int eventIdIndex = 0;
        private ManagedWebSocket socket;

        private readonly Queue<string> incomingAudio = new();

        #endregion

        #region Properties

        private string eventId { get { eventIdIndex++; return $"{eventIdPrepend}{eventIdIndex}"; } } //Property for event indexing because I'm lazy

        #endregion

        #region Public Methods

        [ExposeInInspector]
        public async Task Initialize() {
            socket = new ManagedWebSocket(realtimeEndpoint, new Dictionary<string, string>() {
                { "Authorization",$"Bearer {apiKey.Key}" },
                { "OpenAI-Beta", "realtime=v1" }
            });

            socket.OnOpen += OnOpen;
            socket.OnClose += OnClose;
            socket.OnMessage += OnMessage;
            socket.OnPartialMessage += OnPartialMessage;

            await socket.Open();
        }

        [ExposeInInspector]
        public async Task Stop() {
            await socket.Close(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure);
        }

        public async void OnOpen() {
            await UpdateSession();
            //await CreateConversationItem("Come ti chiami?"); //For testing only!
            //await RequestResponse();
        }

        public void OnClose() {
            Log.D($"Socket closed");
        }

        public async void OnMessage(string message) {
            var json = JObject.Parse(message);
            var typeValue = json.Value<string>("type");
            switch (typeValue) {
                case "session.created":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "session.updated":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "conversation.created":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "conversation.item.created":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "conversation.item.input_audio_transcription.completed":
                    Log.D($"Server: {typeValue} -> {json.Value<string>("transcript")}");
                    break;
                case "conversation.item.input_audio_transcription.failed":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "conversation.item.truncated":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "conversation.item.deleted":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "input_audio_buffer.committed":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "input_audio_buffer.cleared":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "input_audio_buffer.speech_started":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "input_audio_buffer.speech_stopped":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "response.created":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "response.done":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "rate_limits.updated":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "response.content_part.added":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "response.audio.delta":
                    //Log.D($"Server: {typeValue}.");
                    var base64Response = json.Value<string>("delta");
                    ThreadDispatcher.Instance.DispatchOnMainThread(() => onPartialBase64Response.Invoke(base64Response));
                    break;
                case "response.text.done":
                    Log.D($"Server: {typeValue} -> {json.Value<string>("text")}.");
                    break;
                case "response.audio_transcript.done":
                    Log.D($"Server: {typeValue} -> {json.Value<string>("transcript")}.");
                    break;
                default:
                    break;
            }
        }

        public void OnPartialMessage(string message) {
            //Log.D($"Received partial message: {message}");
            //var clip = message.GetAudioClip();
            //onPartialResponse?.Invoke(clip);
        }

        [ExposeInInspector]
        public async void Send(AudioClip clip) {
            await AppendAudio(clip);
            await CommitAudio();
            await RequestResponse();
        }

        #endregion

        #region Unity Overrides

        protected override async void Awake() {
            base.Awake();

            //Sanity checks
            if (apiKey == null) throw new ReferenceMissingException(nameof(apiKey));
            if (configuration == null) throw new ReferenceMissingException(nameof(configuration));

            //Initialization
            if (initializationType == InitializationTypes.OnAwake)
                await Initialize();
        }

        private async void Start() {
            if (initializationType == InitializationTypes.OnStart)
                await Initialize();
        }

        private async void OnApplicationQuit() {
            await socket.Close();
        }

        #endregion

        #region Private Methods

        private async Task CreateConversationItem(string text) {
            var jsonCreateConversationItem = new {
                event_id = eventId,
                type = "conversation.item.create",
                item = new {
                    type = "message",
                    role = "user",
                    content = new[] { new {
                        type = "input_text",
                        text = text
                        }
                    }
                }
            };

            var request = JsonConvert.SerializeObject(jsonCreateConversationItem);
            await socket.Send(request);
        }

        /// <summary>
        /// Updates current assistant session with current settings.
        /// </summary>
        private async Task UpdateSession() {
            var jsonUpdateSessionRequest = new {
                event_id = eventId,
                type = "session.update",
                session = new {
                    modalities = (realtimeMode == OpenAIRealtimeModes.Text_And_Audio) ? new[] { "text", "audio" } : new[] { "text" } , 
                    instructions = configuration.MessagesString,
                    voice = voice.GetStringValue(),
                    input_audio_format = inputAudioFormat.GetStringValue(),
                    output_audio_format = outputAudioFormat.GetStringValue(),
                    input_audio_transcription = new { model = "whisper-1" }, //TODO this can maybe be null because the RT model works also with just audio.
                    turn_detection = (voiceActivationDetection == OpenAIVAD.None) ? null : new {
                        type = "server_vad",
                        threshold = 0.5f,
                        prefix_padding_ms = 300,
                        silence_duration_ms = 500
                    }
                }
            };
            var request = JsonConvert.SerializeObject(jsonUpdateSessionRequest);
            Log.D($"Sending request {request}");
            await socket.Send(request);
        }

        /// <summary>
        /// Requests a response from previous appended Audio or text. Doesn't need to be called if the server is in VAD mode.
        /// </summary>
        private async Task RequestResponse() {
            var jsonRequestResponse = new {
                event_id = eventId,
                type = "response.create"
            };
            var request = JsonConvert.SerializeObject(jsonRequestResponse);
            Log.D($"Sending response request: {request}");
            await socket.Send(request);
        }

        /// <summary>
        /// Appends audio to the buffer
        /// </summary>
        private async Task AppendAudio(AudioClip clip) {
            Log.D($"Trying to append audio");
            var clipBase64 = clip.GetBase64();

            var jsonAppendAudioRequest = new {
                event_id = eventId,
                type = "input_audio_buffer.append",
                audio = clipBase64
            };

            var request = JsonConvert.SerializeObject(jsonAppendAudioRequest);
            Log.D($"Sending append audio request: {request}");
            await socket.Send(request);
        }

        /// <summary>
        /// Clears appended audio in the buffer
        /// </summary>
        private async Task ClearAudio() {
            Log.D($"Requesting audio clear.");
            var jsonClearAudioRequest = new {
                event_id = eventId,
                type = "input_audio_buffer.cleared"
            };
            var request = JsonConvert.SerializeObject(jsonClearAudioRequest);
            await socket.Send(request);
        }

        /// <summary>
        /// Commits previously appended audio to the buffer
        /// </summary>
        private async Task CommitAudio() { //No need if the server goes into VAD mode
            Log.D($"Requesting audio commit.");
            var jsonCommitAudioRequest = new {
                event_id = eventId,
                type = "input_audio_buffer.commit"
            };
            var request = JsonConvert.SerializeObject(jsonCommitAudioRequest);
            Log.D($"Sending commit audio request: {request}");
            await socket.Send(request);
        }

        #endregion

    }
}

#endif