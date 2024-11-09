using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.Contracts;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO {

    /// <summary>
    /// Base recorder service. Records user's mic trying to eliminate silence.
    /// </summary>
    public abstract class BaseRecorder : MonoBehaviour, IRecorder {

        [Header("Parameters")]
        [Tooltip("Treshold under which the recording counts as silence")]
        [SerializeField] protected float loudnessThreshold = 0.01f;
        [Tooltip("How long sound should be under the loudness threshold to be considered silence")]
        [SerializeField] protected float silenceDuration = 2.0f;
        [SerializeField] protected int sampleWindow = 64;
        [SerializeField] protected int sampleRate = 44100;
        [Range(3.0f, 10.0f), SerializeField] protected int maxRecordingLength = 5;

        [Header("Events")]
        [Tooltip("Event fired when the transcription is ready for being used")]
        [SerializeField] protected UnityEvent<AudioClip> onRecordingReady = new UnityEvent<AudioClip>();

        [Header("ReadOnly Parameters")]
        [Tooltip("The microphone it's currently using")]
        [ReadOnly, SerializeField] private string micInUse = "";
        [Tooltip("Loudness level")]
        [ReadOnly, SerializeField] protected float currentLoudness = -1f;
        [Tooltip("Wheter it's recording or not")]
        [ReadOnly, SerializeField] protected bool isRecording = false;

        protected float[] buffer;
        protected float lastLoudTime;
        protected AudioClip recording;

        public bool IsRecording => isRecording;
        public UnityEvent<AudioClip> OnRecordingAvailable => onRecordingReady;

        public virtual void StartVoiceRecording() {
            if (isRecording)
                return;
            recording = Microphone.Start(micInUse, true, maxRecordingLength, sampleRate);
            if (recording == null) {
                Log.E("Something went wrong...");
            }
            isRecording = true;
            lastLoudTime = Time.realtimeSinceStartup;
        }

        public virtual void StopVoiceRecording() {
            if (!isRecording)
                return;
            Microphone.End(micInUse);
            isRecording = false;
            onRecordingReady.Invoke(recording);
        }

        private void Start() {
            buffer = new float[sampleRate * maxRecordingLength];
            micInUse = Microphone.devices[0];
            foreach (var device in Microphone.devices) {
                Log.D($"Found mic [{device}]");
                if (device.Contains("EPOS")) { //TODO delete this or change it to something parametrizable.
                    micInUse = device;
                } else if (device.Contains("Realtek")) {
                    micInUse = device;
                }
            }
            var audioConfig = AudioSettings.GetConfiguration();
            sampleRate = audioConfig.sampleRate;
            Initialize();
        }

        protected virtual void Update() {
            if (isRecording) {
                AnalyzeMicrophoneInput();
            }
        }

        private void OnDisable() {
            StopVoiceRecording();
        }

        protected virtual void AnalyzeMicrophoneInput() {
            var offsetSamples = Microphone.GetPosition(micInUse) % buffer.Length;
            recording.GetData(buffer, sampleWindow);
            currentLoudness = GetLoudness(recording, sampleWindow);

            if (currentLoudness > loudnessThreshold) {
                lastLoudTime = Time.realtimeSinceStartup;
            } else if (Time.realtimeSinceStartup - lastLoudTime > silenceDuration) {
                Log.D("Detected silence, stopping recording.");
                StopVoiceRecording();
            }
        }

        protected float GetLoudness(AudioClip Clip, int ClipPosition) {
            int startPosition = ClipPosition - sampleWindow;
            if (startPosition < 0)
                return 1; //TODO fix
            var data = new float[sampleWindow];
            Clip.GetData(data, startPosition);

            float loudness = 0;
            for (int i = 0; i < sampleWindow; i++) {
                loudness += MathF.Abs(data[i]);
            }

            return loudness / sampleWindow; //RMS
        }

        public abstract void Initialize();

    }

}