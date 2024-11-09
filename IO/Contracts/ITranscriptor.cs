using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO.Contracts {

    /// <summary>
    /// General interface for voice transcription services (a.k.a. Speech To Text services)
    /// </summary>
    public interface ITranscriptor {

        /// <summary>
        /// Callback used when the text of the transcription is available
        /// </summary>
        UnityEvent<string> OnTranscriptionReady { get; }

        /// <summary>
        /// Initialization method
        /// </summary>
        void Initialize();
        
        void ProcessAudio(AudioClip Recording);
    }

}
