using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.AzureDataStructures;
using Castrimaris.IO.Contracts;
using Castrimaris.ScriptableObjects;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO {

    /// <summary>
    /// Implementation of the Text to Speech service of Azure Cognitive Services.
    /// </summary>
    public class AzureTextToSpeech : MonoBehaviour, ITextToSpeech {

        private const int FREQUENCY = 24000; //24KHz
        private const string REGEX_PATTERN = "[.,!?]"; //Cutoff pattern to recognize, so that we can split dialogue efficiently with the sentences.

        [Header("Parameters")]
        [Tooltip("Reference to the API Key necessary for using the service")]
        [SerializeField] private APIKey apiKey;
        [Tooltip("How or when the service should be initialized")]
        [SerializeField] private InitializationTypes initializationType;
        [Tooltip("Target server region for using the service")]
        [SerializeField] private Regions region = Regions.West_Europe;
        [Tooltip("Language of the text to speech")]
        [SerializeField] private Languages language = Languages.Italian;

        [Tooltip("Italian voice")]
        [ConditionalField(nameof(language), Languages.Italian, DisablingTypes.Hidden)]
        [SerializeField] private ItalianVoices italianVoice = ItalianVoices.Lisandro;

        [Tooltip("American voice")]
        [ConditionalField(nameof(language), Languages.American, DisablingTypes.Hidden)]
        [SerializeField] private AmericanVoices americanVoice = AmericanVoices.Ava;

        [Tooltip("Wheter the text to speech service can be used or not")]
        [SerializeField] private bool canSpeak = true;

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private bool isSpeaking = false;

        [Header("References")]
        [Tooltip("Target speaker for the voice")]
        [SerializeField] private AudioSource speaker;

        [Header("Events")]
        [Tooltip("Events called when the speaking starts")]
        [SerializeField] private UnityEvent onStartedSpeaking = new UnityEvent();

        [Tooltip("Events called when the speaking stops")]
        [SerializeField] private UnityEvent onStoppedSpeaking = new UnityEvent();

        private SpeechSynthesizer synth;
        private SpeechConfig config;
        private int partialTextIndex = 0;
        private Queue<string> partialTextQueue = new Queue<string>();
        private Dictionary<int, AudioClip> ttsClips = new Dictionary<int, AudioClip>();
        private Coroutine playQueueCoroutine = null;

        public bool IsSpeaking => isSpeaking;
        public bool CanSpeak { get => canSpeak; set => canSpeak = value; }
        public UnityEvent OnStartedSpeaking => onStartedSpeaking;
        public UnityEvent OnStoppedSpeaking => onStoppedSpeaking;

        /// <summary>
        /// Speaks a given text.
        /// </summary>
        /// <param name="Text">The text to speak</param>
        public async void Speak(string Text) {
            if (!canSpeak)
                return;
            if (Text.IsNullOrEmpty())
                return;
            var clip = await GenerateAsync(Text);
            speaker.clip = clip;
            speaker.Play();
        }

        /// <summary>
        /// Enqueues a text to be spoken.
        /// </summary>
        /// <param name="Text">The text to speak</param>
        public void SpeakEnqueue(string Text) {
            if (!canSpeak)
                return;
            if (Text.IsNullOrEmpty())
                return;
            partialTextQueue.Enqueue(Text);
            if (Regex.IsMatch(Text, REGEX_PATTERN)) { //starts speaking a phrase only if a certain pattern emerges from the regular expression (namely if it detects a point, a comma or any other phrase ending character.
                var phrase = string.Join("", partialTextQueue.ToArray());
                Log.D($"Enqueuing phrase {phrase}");
                partialTextQueue.Clear();
                GenerateEnqueuedSpeech(phrase);
            }
        }

        /// <summary>
        /// Generates an <see cref="AudioClip"/> of a given text
        /// </summary>
        /// <param name="Text">The text to speak</param>
        /// <returns>An <see cref="AudioClip"/> of the given text if successful, null otherwise</returns>
        public async Task<AudioClip> Generate(string Text) {
            var synthesisResult = await synth.SpeakTextAsync(Text);
            var dataStream = AudioDataStream.FromResult(synthesisResult);
            int clipDuration = (int)(System.Math.Round(synthesisResult.AudioDuration.TotalSeconds, 0, MidpointRounding.ToEven)) * FREQUENCY;
            Log.D($"Generated audio from Text '{Text}' of length {synthesisResult.AudioDuration.TotalSeconds}, presumed duration {(int)(System.Math.Round(synthesisResult.AudioDuration.TotalSeconds, 0, MidpointRounding.ToEven))}");
            byte[] buffer = new byte[clipDuration * 2];
            float[] data = new float[clipDuration];
            dataStream.ReadData(buffer);
            for (int i = 0; i < clipDuration; ++i) {
                data[i] = ((short)(buffer[i * 2 + 1] << 8 | buffer[i * 2])) / 32768.0f;
            }
            var clip = AudioClip.Create($"AzureTTS-{partialTextIndex}", clipDuration, 1, FREQUENCY, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Generates instantly an <see cref="AudioClip"/> of a given text, then it's updated as data arrives.
        /// </summary>
        /// <param name="Text">The text to speak</param>
        /// <returns>An <see cref="AudioClip"/> that may or may not be updated as data arrives.</returns>
        public async Task<AudioClip> GenerateAsync(string Text) {
            var synthesisResult = await synth.StartSpeakingTextAsync(Text);
            var dataStream = AudioDataStream.FromResult(synthesisResult);
            int clipDuration = (int)(System.Math.Round(synthesisResult.AudioDuration.TotalSeconds, 0, MidpointRounding.ToEven)) * FREQUENCY; //Calculate the clip duration by rounding the total seconds duration estimation. It is possible that, very rarely, some audio can be cutoff this way, but it should be the last few milliseconds of the last syllabe, but we're ok with it as it makes the system very responsive.
            Log.D($"Generating async clip of text '{Text}', duration {synthesisResult.AudioDuration.TotalSeconds}, total suspected duration {clipDuration}");
            var clip = AudioClip.Create($"AzureTTS-{partialTextIndex}", clipDuration, 1, FREQUENCY, true, (audioChunk) => OnPCMAudioAvailable(audioChunk, dataStream)); //We create an AudioClip which receives the audio as the PCM is available; each audiochunk gets added to the AudioClip as it becomes avaiable.
            return clip;
        }

        private void Awake() {
            if (speaker == null)
                throw new MissingReferenceException($"No reference set for {nameof(speaker)}. Did you forget to assign it in the Editor?");
            if (initializationType == InitializationTypes.OnAwake) 
                Initialize();
        }

        private void Start() {
            if (initializationType == InitializationTypes.OnStart)
                Initialize();
        }

        private void OnDestroy() {
            synth.Dispose();
        }

        private void Initialize() {
            config = SpeechConfig.FromSubscription(apiKey.Key, region.AsString());
            string chosenVoice = null;
            switch (language) {
                case Languages.Italian:
                    chosenVoice = italianVoice.AsString();
                    break;
                case Languages.American:
                    chosenVoice = americanVoice.AsString();
                    break;
                default:
                    chosenVoice = italianVoice.AsString();
                    break;
            }
            config.SpeechSynthesisLanguage = language.AsString();
            config.SpeechSynthesisVoiceName = chosenVoice;
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);
            synth = new SpeechSynthesizer(config, null); //null because otherwise it will try to play audio natively; this means that it will play audio using Android's OS audio systems if on Android, or Windows OS systems on Windows.

            synth.SynthesisCanceled += SpeechSynthesisErrorHandler; //We handle errors as they arise
        }

        private void SpeechSynthesisErrorHandler(object sender, SpeechSynthesisEventArgs args) {
            var details = SpeechSynthesisCancellationDetails.FromResult(args.Result);
            Log.E($"Fatal error while trying to synthesize audio. {details.Reason}.\n\n{details.ErrorDetails}.");
        }

        private async void GenerateEnqueuedSpeech(string Phrase) {
            int index = ttsClips.Count;
            ttsClips.Add(index, null);
            var clip = await Generate(Phrase);
            ttsClips[index] = clip;
            if (playQueueCoroutine == null) {
                playQueueCoroutine = StartCoroutine(PlayEnqueued());
            }
        }

        private IEnumerator PlayEnqueued() {
            isSpeaking = true;
            onStartedSpeaking.Invoke();
            while (partialTextIndex < ttsClips.Count) {
                while (ttsClips[partialTextIndex] == null) {
                    yield return null;
                }
                speaker.clip = ttsClips[partialTextIndex];
                speaker.Play();
                while (speaker.isPlaying)
                    yield return null;
                partialTextIndex++;
            }
            partialTextIndex = 0;
            ttsClips.Clear();
            playQueueCoroutine = null;
            isSpeaking = false;
            onStoppedSpeaking.Invoke();
        }

        private void OnPCMAudioAvailable(float[] audioChunk, AudioDataStream audioDataStream) {
            var chunkSize = audioChunk.Length; //Get the size of the chunk so to get the number of samples
            var audioChunkBytes = new byte[chunkSize * 2]; //Container of the audio data with each sample being 2bytes for 16bit audio
            var readBytes = audioDataStream.ReadData(audioChunkBytes); //data reading
            for (int i = 0; i < chunkSize; ++i) {
                if (i < readBytes / 2) { //if there's data to read, do it.
                    audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F; //Combine the two bytes of audio into a single 16bit short, then normalize it by dividing for the max value of short (which is, in fact, 32768)
                } else { //otherwise the samples should be set to zero (no audio
                    audioChunk[i] = 0.0f;
                }
            }
        }
    }
}

