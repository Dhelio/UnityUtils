using Castrimaris.Attributes;
using Castrimaris.Core;
using Castrimaris.IO.ScriptableObjects;
using Castrimaris.ScriptableObjects;
using UnityEngine.Events;
using UnityEngine;
using Castrimaris.Core.Monitoring;
using System;
using System.Threading.Tasks;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Utilities;
using Newtonsoft.Json;
using Castrimaris.Network;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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

        private Queue<string> incomingAudio = new Queue<string>();

        #endregion

        #region Properties

        private string eventId { get { eventIdIndex++; return $"{eventIdPrepend}{eventIdIndex}"; } } //Property for event indexing because I'm lazy

        #endregion

        #region Public Methods

        [ExposeInInspector]
        public async Task Initialize() {
            socket = new ManagedWebSocket(realtimeEndpoint, new System.Collections.Generic.Dictionary<string, string>() {
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
            await CreateConversationItem("Come ti chiami?");
            await RequestResponse();
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
                case "conversation.item.created":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "response.created":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "rate_limits.updated":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "response.content_part.added":
                    Log.D($"Server: {typeValue}.");
                    break;
                case "response.audio.delta":
                    Log.D("Received audio delta.");
                    var base64Response = json.Value<string>("delta");
                    ThreadDispatcher.Instance.DispatchOnMainThread(() => onPartialBase64Response.Invoke(base64Response));
                    //incomingAudio.Enqueue(json.Value<string>("delta"));
                    //ThreadDispatcher.Instance.DispatchOnMainThread( () => {
                    //    var deltaBase64 = json.Value<string>("delta");
                    //    var clip = deltaBase64.GetAudioClip();
                    //    onPartialResponse.Invoke(clip);
                    //});
                    break;
                case "response.done":
                    //var base64Audio = string.Join("", incomingAudio);
                    //incomingAudio.Clear();
                    //ThreadDispatcher.Instance.DispatchOnMainThread(() => {
                    //    var clip = base64Audio.GetAudioClip();
                    //    onResponse.Invoke(clip);
                    //});
                    break;
                default:
                    break;
            }

            //var clip = message.GetAudioClip();
            //onResponse?.Invoke(clip);
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

            if (apiKey == null) {
                Log.E($"No API key set for {nameof(Realtime)}! Please, assign one in the Editor.");
                return;
            }

            if (configuration == null) {
                Log.E($"No configuration set for {nameof(Realtime)}! Please, assign one in the Editor.");
            }

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

        private async Task UpdateSession() {
            var jsonUpdateSessionRequest = new {
                event_id = eventId,
                type = "session.update",
                session = new {
                    modalities = (realtimeMode == OpenAIRealtimeModes.Text_And_Audio) ? new[] { "text", "audio" } : new[] { "text" } , 
                    instructions = configuration.MessagesString,
                    voice = voice.AsString(),
                    input_audio_format = inputAudioFormat.AsString(),
                    output_audio_format = outputAudioFormat.AsString(),
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

        private async Task RequestResponse() {
            var jsonRequestResponse = new {
                event_id = eventId,
                type = "response.create"
            };
            var request = JsonConvert.SerializeObject(jsonRequestResponse);
            Log.D($"Sending response request: {request}");
            await socket.Send(request);
        }

        private async Task AppendAudio(AudioClip clip) {
            Log.D($"Trying to append audio");
            var clipBytes = AudioClipUtilities.GetBytesFromClip(clip);
            var clipBase64 = Convert.ToBase64String(clipBytes);

            var jsonAppendAudioRequest = new {
                event_id = eventId,
                type = "input_audio_buffer.append",
                audio = clipBase64
            };

            var request = JsonConvert.SerializeObject(jsonAppendAudioRequest);
            Log.D($"Sending append audio request: {request}");
            await socket.Send(request);
        }

        private async Task ClearAudio() {
            Log.D($"Requesting audio clear.");
            var jsonClearAudioRequest = new {
                event_id = eventId,
                type = "input_audio_buffer.cleared"
            };
            var request = JsonConvert.SerializeObject(jsonClearAudioRequest);
            await socket.Send(request);
        }

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