using Assets.Temp;
using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.AzureDataStructures;
using Castrimaris.ScriptableObjects;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Speaker;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Castrimaris.IO {

    public class AzureSpeakerRecognizer : MonoBehaviour {

        #region Classes

        /// <summary>
        /// Integration of the <see cref="PullAudioInputStreamCallback"/> class with Unity's <see cref="AudioClip"/> system.
        /// </summary>
        public class AudioClipInputStreamCallback : PullAudioInputStreamCallback {

            private readonly AudioClip clip;
            private readonly float[] data;
            private IntPtr dataPointer;
            private int position;
            private GCHandle handle;

            public AudioClipInputStreamCallback(AudioClip clip) {
                this.clip = clip ?? throw new NullReferenceException($"Tried to build a {nameof(AudioClipInputStreamCallback)} but the passed AudioClip is null! Ensure that the AudioClip is valid");
                data = new float[clip.samples * clip.channels];
                clip.GetData(data, 0);
                position = 0;
                handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                dataPointer = handle.AddrOfPinnedObject();
                position = 0;
            }

            public override int Read(byte[] dataBuffer, uint size) {
                int floatSize = sizeof(float);
                int sampleCount = (int)size / floatSize;

                if (position >= data.Length) {
                    return 0; // No more data to read
                }

                int samplesToCopy = Mathf.Min(sampleCount, data.Length - position);

                Marshal.Copy(dataPointer + position * floatSize, dataBuffer, 0, samplesToCopy * floatSize);
                position += samplesToCopy;

                return samplesToCopy * floatSize;
            }

            public override void Close() {
                if (handle.IsAllocated) {
                    handle.Free();
                }

                base.Close();
            }
        }

        #endregion

        #region Private Variables
        [Header("Parameters")]
        [Tooltip("Reference to the API Key necessary for using the service")]
        [SerializeField] private APIKey apiKey;
        [Tooltip("How or when the service should be initialized")]
        [SerializeField] private InitializationTypes initializationType;
        [Tooltip("Target server region for using the service")]
        [SerializeField] private Regions region = Regions.West_Europe;

        private SpeechConfig config;
        private VoiceProfileClient voiceProfileClient;
        private VoiceProfile voiceProfile;
        private bool isEnrolled = false;
        private bool isInitialized = false;
        private string enrollmentId = "";

        #endregion

        #region Public Methods

        public async Task Recognize(AudioClip clip) {
            if (!isInitialized)
                return;

            if (!isEnrolled) {
                Log.W($"Tried to recognize user, but enrollment isn't done yet! Using recording to enroll the user instead...");
                await Enroll(clip);
            }

            //TODO recognize

            await Task.CompletedTask;
        }

        public async Task Enroll(AudioClip clip) {
            if (!isInitialized)
                return;

            var _ = await voiceProfileClient.GetActivationPhrasesAsync(VoiceProfileType.TextIndependentVerification, region.AsString());
            var audioConfig = AudioConfig.FromStreamInput(new AudioClipInputStreamCallback(clip));
            var enrollmentResult = await voiceProfileClient.EnrollProfileAsync(voiceProfile, audioConfig);
            if (enrollmentResult.RemainingEnrollmentsSpeechLength > TimeSpan.Zero) { 
                Log.D($"Successfully used audio sample for partial Enrollment process, {enrollmentResult.RemainingEnrollmentsSpeechLength} seconds remain to enroll.");
            } else {
                Log.D($"Data complete, successfully enrolled user.");
                isEnrolled = true;
            }
        }

        #endregion

        #region Unity Overrides

        private async void Awake() {
            if (initializationType == InitializationTypes.OnAwake)
                await Initialize();
        }

        private async void Start() {
            if (initializationType == InitializationTypes.OnStart)
                await Initialize();
        }

        #endregion

        #region Private Methods

        private async Task Initialize() {
            if (isInitialized)
                return;

            var enrollmentId = $"{Prefixes.Rand()} {Suffixes.Rand()}";

            Log.D($"Initializing with key {apiKey.Key}, region {region.AsString()}, id {enrollmentId}");

            config = SpeechConfig.FromSubscription(apiKey.Key, region.AsString());
            this.enrollmentId = enrollmentId;
            voiceProfileClient = new VoiceProfileClient(config);
            voiceProfile = await voiceProfileClient.CreateProfileAsync(VoiceProfileType.TextIndependentVerification, region.AsString());

            isInitialized = true;

            await Task.CompletedTask;
        }

        #endregion

    }
}


