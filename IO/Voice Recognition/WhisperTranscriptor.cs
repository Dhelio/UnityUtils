#if OPENAI

using Castrimaris.Attributes;
using OpenAI.Audio;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO
{
    /// <summary>
    /// Implementation of Rage Against The Pixel's OpenAI API for the transcription models.
    /// </summary>
    [RequireComponent(typeof(OpenAIApi))]
    public class WhisperTranscriptor : BaseTranscriptor {

        #region Private Variables

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private ulong transcriptionIndex = 0;

        [Header("Events")]
        [SerializeField] private UnityEvent onTranscriptionRequestSent = new UnityEvent();

        private OpenAIApi chatGPT;
        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region Properties

        public UnityEvent OnTrascriptionRequestSent => onTranscriptionRequestSent;

        #endregion

        #region Public Methods

        public override void Initialize() {
            chatGPT = GetComponent<OpenAIApi>();
        }

        public override void ProcessAudio(AudioClip recording) {
            var request = new AudioTranscriptionRequest(recording);
            SendTranscriptionRequest(request);
            transcriptionIndex++;
        }

        [ExposeInInspector]
        public void SimulateTranscription(string Value) {
            OnTranscriptionReady.Invoke(Value);
        }

        [ExposeInInspector]
        public void Abort() {
            cancellationTokenSource.Cancel();
        }

        #endregion

        #region Private Methods

        private async void SendTranscriptionRequest(AudioTranscriptionRequest request) {
            cancellationTokenSource = new CancellationTokenSource();
            onTranscriptionRequestSent.Invoke();
            var result = await chatGPT.Client.AudioEndpoint.CreateTranscriptionAsync(request, cancellationTokenSource.Token);
            if (cancellationTokenSource.IsCancellationRequested)
                return;
            OnTranscriptionReady.Invoke(result);
        }

        #endregion

    }

}

#endif