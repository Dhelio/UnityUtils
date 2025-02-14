using Castrimaris.Attributes;
using Castrimaris.Core.Extensions;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.Contracts;
using OpenAI.Audio;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO {

    /// <summary>
    /// Implementation of Rage Against The Pixel's OpenAI API for the text to speech models.
    /// </summary>
    [RequireComponent(typeof(OpenAIApi))]
    public class OpenAITextToSpeech : MonoBehaviour, ITextToSpeech {

        [Header("Parameters")]
        [SerializeField] private SpeechVoice voice = SpeechVoice.Echo;
        [SerializeField] private bool canSpeak = true;

        [Header("References")]
        [SerializeField] private AudioSource speakingPoint;

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private bool isSpeaking = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onStartedSpeaking = new UnityEvent();
        [SerializeField] private UnityEvent onStoppedSpeaking = new UnityEvent();

        private OpenAIApi chatGPT;
        private Queue<string> partialTextQueue = new Queue<string>();
        private Dictionary<int, AudioClip> ttsClips = new Dictionary<int, AudioClip>();
        private Coroutine playQueueCoroutine = null;
        private int currentIndex = 0;
        private CancellationTokenSource cancellationTokenSource;

        private const string REGEX_PATTERN = "[.,!?]";

        public bool IsSpeaking => isSpeaking;
        public bool CanSpeak { get => canSpeak; set => canSpeak = value; }
        public UnityEvent OnStartedSpeaking => onStartedSpeaking;
        public UnityEvent OnStoppedSpeaking => onStoppedSpeaking;

        public void SpeakEnqueue(string Text) {
            if (!canSpeak)
                return;
            if (Text.IsNullOrEmpty())
                return;
            partialTextQueue.Enqueue(Text);
            if (Regex.IsMatch(Text, REGEX_PATTERN)) {
                var phrase = string.Join("", partialTextQueue.ToArray());
                Log.D($"Enqueuing phrase {phrase}");
                partialTextQueue.Clear();
                var request = new SpeechRequest(input: phrase, voice: voice);
                GenerateEnqueuedSpeech(request);
            }
        }

        public void Speak(string Text) {
            if (!canSpeak)
                return;
            if (Text.IsNullOrEmpty())
                return;
            var request = new SpeechRequest(input: Text, voice: voice);
            GenerateSpeech(request);
        }

        public async Task<AudioClip> Generate(string Text) {
            var request = new SpeechRequest(input: Text, voice: voice);
            var result = await chatGPT.Client.AudioEndpoint.CreateSpeechAsync(request, cancellationTokenSource.Token);
            if (cancellationTokenSource.IsCancellationRequested) {
                StopCoroutine(playQueueCoroutine);
                playQueueCoroutine = null;
                ttsClips.Clear();
                cancellationTokenSource = new CancellationTokenSource();
                return null;
            }
            var clip = result.Item2;
            return clip;
        }

        public Task<AudioClip> GenerateAsync(string Text) {
            //TODO
            throw new System.NotImplementedException();
        }

        public void Abort() {
            cancellationTokenSource.Cancel();
        }

        private void Awake() {
            chatGPT = GetComponent<OpenAIApi>();
            if (speakingPoint == null)
                speakingPoint = this.gameObject.AddComponent<AudioSource>();

            cancellationTokenSource = new CancellationTokenSource();
        }

        //TODO temp remove me please!
        //private async void Start() {
        //    await GenerateAndSaveClip("Wait1", "Dammi un momento...");
        //    await GenerateAndSaveClip("Wait2", "Un momento solo...");
        //    await GenerateAndSaveClip("Wait3", "Solo un istante...");
        //}

        private async void GenerateSpeech(SpeechRequest Request) {
            var result = await chatGPT.Client.AudioEndpoint.CreateSpeechAsync(Request);
            speakingPoint.clip = result.Item2;
            speakingPoint.Play();
            StartCoroutine(CheckSpeechCoroutine());
        }

        private IEnumerator CheckSpeechCoroutine() {
            isSpeaking = true;
            onStartedSpeaking.Invoke();
            var wfs = new WaitForSeconds(.5f);
            while (speakingPoint.isPlaying) {
                yield return wfs;
            }
            isSpeaking = false;
            onStoppedSpeaking.Invoke();
        }

        private async void GenerateEnqueuedSpeech(SpeechRequest Request) {
            int index = ttsClips.Count;
            ttsClips.Add(index, null);
            var result = await chatGPT.Client.AudioEndpoint.CreateSpeechAsync(Request);
            ttsClips[index] = result.Item2;
            if (playQueueCoroutine == null) {
                playQueueCoroutine = StartCoroutine(PlayEnqueued());
            }
        }

        private IEnumerator PlayEnqueued() {
            isSpeaking = true;
            onStartedSpeaking.Invoke();
            while (currentIndex < ttsClips.Count) {
                while (ttsClips[currentIndex] == null) {
                    yield return null;
                }
                speakingPoint.clip = ttsClips[currentIndex];
                speakingPoint.Play();
                while (speakingPoint.isPlaying) {
                    yield return null;
                }
                currentIndex++;
            }
            currentIndex = 0;
            ttsClips.Clear();
            playQueueCoroutine = null;
            isSpeaking = false;
            onStoppedSpeaking.Invoke();
        }
    }
}