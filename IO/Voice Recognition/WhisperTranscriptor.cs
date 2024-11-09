using Castrimaris.Attributes;
using Castrimaris.Core.Collections;
using Castrimaris.Core.Monitoring;
using Castrimaris.ScriptableObjects;
using OpenAI;
using OpenAI.Audio;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO
{
    /// <summary>
    /// Implementation of Rage Against The Pixel's OpenAI API for the transcription models.
    /// </summary>
    [RequireComponent(typeof(OpenAIApi))]
    public class WhisperTranscriptor : BaseTranscriptor {

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private ulong transcriptionIndex = 0;

        [Header("Events")]
        [SerializeField] private UnityEvent onTranscriptionRequestSent = new UnityEvent();

        private OpenAIApi chatGPT;

        public UnityEvent OnTrascriptionRequestSent => onTranscriptionRequestSent;

        public override void Initialize() {
            chatGPT = GetComponent<OpenAIApi>();
        }

        public override void ProcessAudio(AudioClip Recording) {
            var request = new AudioTranscriptionRequest(Recording);
            SendTranscriptionRequest(request, transcriptionIndex);
            transcriptionIndex++;
        }

        [ExposeInInspector]
        public void SimulateTranscription(string Value) {
            OnTranscriptionReady.Invoke(Value);
        }

        private async void SendTranscriptionRequest(AudioTranscriptionRequest Request, ulong Index) {
            onTranscriptionRequestSent.Invoke();
            var result = await chatGPT.Client.AudioEndpoint.CreateTranscriptionAsync(Request);
            OnTranscriptionReady.Invoke(result);
        }

    }

}
