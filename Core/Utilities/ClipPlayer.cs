using Castrimaris.Attributes;
using Castrimaris.Core.Exceptions;
using Castrimaris.Core.Monitoring;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.Core {

    /// <summary>
    /// Simple class that provides functionalities relative to clip playing.
    /// </summary>
    public class ClipPlayer : MonoBehaviour {

        [Header("Parameters")]
        [SerializeField] private bool isEnabled = true;
        [Tooltip("Wheter the field clips should be cleared when done playing them throuh PlayEnqueue method")]
        [SerializeField] private bool clearClipsOnPlayEnqueueCompletion = false;
        [Tooltip("Clips that the class has readily available.")]
        [SerializeField] private List<AudioClip> clips = new List<AudioClip>();

        [Header("References")]
        [SerializeField] private AudioSource targetAudioSource;

        private ConcurrentQueue<float> streamingQ = new ConcurrentQueue<float>();

        private Coroutine playEnqueueBehaviour = null;
        private bool isStreaming = false;

        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        public bool Loop { get => targetAudioSource.loop; set => targetAudioSource.loop = value; }

        public void Abort() {
            targetAudioSource.Stop();
            streamingQ.Clear();
        }

        public void Enqueue(string base64Audio) {
            if (!isEnabled)
                return;

            var bytes = Convert.FromBase64String(base64Audio);
            var samples = bytes.Length / 2;
            var floats = new float[samples];
            for (int i = 0; i < floats.Length; i++) {
                var sample = BitConverter.ToInt16(bytes, i * 2);
                floats[i] = sample / 32768f;
            }
            foreach (var f in floats)
                streamingQ.Enqueue(f);
        }

        public void Stream() {
            if (!isEnabled)
                return;

            if (isStreaming) {
                return;
            }

            isStreaming = true;
            var clip = AudioClip.Create("StreamingAudio", 44100 * 3, 1, 44100, true, OnAudioStreaming);
            targetAudioSource.clip = clip;
            targetAudioSource.loop = true;
            targetAudioSource.Play();
        }

        public void Stream(string base64Audio) {
            if (!isEnabled)
                return;

            Enqueue(base64Audio);
            Stream();
        }

        /// <summary>
        /// Adds <see cref="AudioClip">AudioClips</see> to later play
        /// </summary>
        /// <param name="clips"></param>
        public void Enqueue(params AudioClip[] clips) {
            if (!isEnabled)
                return;

            this.clips.AddRange(clips);
        }

        /// <summary>
        /// Enqueues <see cref="AudioClip">Audioclips</see> and then plays from the top (if it's not already playing).
        /// </summary>
        /// <param name="clips"></param>
        public void PlayEnqueue(params AudioClip[] clips) {
            if (!isEnabled)
                return;

            Enqueue(clips);
            if (playEnqueueBehaviour == null)
                playEnqueueBehaviour = StartCoroutine(PlayEnqueueBehaviour());
        }

        /// <summary>
        /// Enqueues this <see cref="AudioClip"/> and then plays from the top (if it's not already playing).
        /// </summary>
        public void PlayEnqueue(AudioClip clip) {
            if (!isEnabled)
                return;

            Enqueue(clip);
            if (playEnqueueBehaviour == null)
                playEnqueueBehaviour = StartCoroutine(PlayEnqueueBehaviour());
        }



        /// <summary>
        /// Plays a random <see cref="AudioClip"/>.
        /// </summary>
        public void PlayRandom() {
            if (!isEnabled)
                return;

            targetAudioSource.clip = clips[UnityEngine.Random.Range(0, clips.Count)];
            targetAudioSource.Play();
        }

        /// <summary>
        /// Plays this specific <see cref="AudioClip"/>.
        /// </summary>
        /// <param name="clip">The <see cref="AudioClip"/> to play</param>
        public void Play(AudioClip clip) {
            if (!isEnabled)
                return;

            targetAudioSource.clip = clip;
            targetAudioSource.Play();
        }

        [ExposeInInspector]
        public void Play() => targetAudioSource.Play();

        [ExposeInInspector]
        public void Stop() {
            if (playEnqueueBehaviour != null)
                StopCoroutine(playEnqueueBehaviour);
            targetAudioSource.Stop();
        }

        private void Awake() {
            if (targetAudioSource == null) {
                if (!TryGetComponent<AudioSource>(out targetAudioSource)) 
                    throw new ReferenceMissingException(nameof(targetAudioSource));
            }
        }

        /// <summary>
        /// Callback for realtime audio streaming
        /// </summary>
        private void OnAudioStreaming(float[] chunk) {
            if (!isEnabled)
                return;

            for (int i = 0; i < chunk.Length; i++) {
                if (!streamingQ.TryDequeue(out var audioBit)) {
                    chunk[i] = 0;
                } else {
                    chunk[i] = audioBit;
                }
            }
        }

        private IEnumerator PlayEnqueueBehaviour() {
            if (!isEnabled)
                yield break;

            for (int i = 0; i < clips.Count; i++) {
                targetAudioSource.clip = clips[i];
                targetAudioSource.Play();
                var wfs = new WaitForSeconds(clips[i].length);
                yield return wfs;
            }

            if (clearClipsOnPlayEnqueueCompletion)
                clips.Clear();

            playEnqueueBehaviour = null;
        }

    }

}
