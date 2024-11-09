using Dissonance;
using Dissonance.Integrations.Offline;
using Castrimaris.Attributes;
using Castrimaris.IO.Contracts;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using FLog = Castrimaris.Core.Monitoring.Log;

namespace Castrimaris.IO {

    /// <summary>
    /// Mic recording retriever for Dissonance
    /// </summary>
    public class DissonanceRecorder : BaseMicrophoneSubscriber, IRecorder {

        [Header("Parameters")]
        [Tooltip("Wheter the recorder should start recording on Start")]
        [SerializeField] private bool shouldRecordOnStart = false;
        [Tooltip("Minimum recording time for a recording to be considered valid")]
        [Range(1.0f, 10.0f), SerializeField] private float minimumRecordingTime = 1.2f;

        [Header("Events")]
        [Tooltip("Event called when the audio recording is available")]
        [SerializeField] private UnityEvent<AudioClip> onRecordingAvailable = new UnityEvent<AudioClip>();

        [Header("ReadOnly Parameters")]
        [ReadOnly, SerializeField] private float startSpokenTime = float.NegativeInfinity;
        [ReadOnly, SerializeField] private bool isRecording = false;
        [ReadOnly, SerializeField] private bool canRecord = false;
        [ReadOnly, SerializeField] private bool isEnabled = true;
        

        private OfflineCommsNetwork offlineComms;
        private DissonanceComms dissonanceComms;
        private Queue<float> audioQ = new Queue<float>();

        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        public bool CanRecord { get => canRecord; set => canRecord = value; }
        public bool IsRecording => isRecording;
        public UnityEvent<AudioClip> OnRecordingAvailable => onRecordingAvailable;


        public void Initialize() {
            canRecord = true;
            dissonanceComms = FindObjectOfType<DissonanceComms>();
            offlineComms = FindObjectOfType<OfflineCommsNetwork>();
        }

        [ExposeInInspector]
        public void StartVoiceRecording() {
            if (isRecording || !isEnabled)
                return;

            isRecording = true;
            dissonanceComms.SubscribeToRecordedAudio(this);
            offlineComms.PlayerStartedSpeaking += HandleRecordingStart;
            offlineComms.PlayerStoppedSpeaking += HandleRecordingStop;

            FLog.D("Started voice recording");
        }

        [ExposeInInspector]
        public void StopVoiceRecording() {
            if (!isRecording || !isEnabled)
                return;

            isRecording = false;
            dissonanceComms.UnsubscribeFromRecordedAudio(this);
            offlineComms.PlayerStartedSpeaking -= HandleRecordingStart;
            offlineComms.PlayerStoppedSpeaking -= HandleRecordingStop;

            FLog.D("Stopped voice recording");
        }

        private void Start() {
            Initialize();
            if (shouldRecordOnStart)
                StartVoiceRecording();
        }

        protected override void ProcessAudio(ArraySegment<float> data) {
            if (!canRecord || !isEnabled)
                return;

            //Get Data
            var dataList = data.ToList();

            //Enqueue data
            dataList.ForEach(element => audioQ.Enqueue(element));
        }

        protected override void ResetAudioStream(WaveFormat waveFormat) => FLog.D($"{nameof(ResetAudioStream)}");

        private void HandleRecordingStart(string _) => startSpokenTime = Time.time;

        private void HandleRecordingStop(string _) => ProcessData();

        private void ProcessData() {
            if (!canRecord || !isEnabled)
                return;

            var spokenTime = Time.time - startSpokenTime;
            if (spokenTime < minimumRecordingTime) {
                FLog.D($"Ignoring recording of {spokenTime} seconds because it's shorter than the required minimum of {minimumRecordingTime} seconds!");
                audioQ.Clear();
                return;
            }

            var clip = AudioClip.Create("Recording", audioQ.Count, 1, 44100, false); //TODO frequency should come from recording device, not hardcoded
            clip.SetData(audioQ.ToArray(), 0);
            onRecordingAvailable.Invoke(clip);
            audioQ.Clear();
        }
    }
}

