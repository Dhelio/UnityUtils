using Castrimaris.Attributes;
using Castrimaris.Core.Monitoring;
using Castrimaris.IO.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO {

    /// <summary>
    /// Base transcription class
    /// </summary>
    public abstract class BaseTranscriptor : MonoBehaviour, ITranscriptor {

        [Header("Events")]
        [SerializeField] private UnityEvent<string> onTrascriptionReady = new UnityEvent<string>();

        public UnityEvent<string> OnTranscriptionReady => onTrascriptionReady;

        protected void Start() {
            Initialize();
        }

        public abstract void ProcessAudio(AudioClip Recording);
        public abstract void Initialize();

    }

}
