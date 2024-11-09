using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO.Contracts {

    /// <summary>
    /// General interface for voice recording services
    /// </summary>
    public interface IRecorder {
    
        /// <summary>
        /// Wheter or not the service is currently recording
        /// </summary>
        bool IsRecording { get; }

        /// <summary>
        /// Callback used when the clip of the recording is available.
        /// </summary>
        UnityEvent<AudioClip> OnRecordingAvailable { get; }

        /// <summary>
        /// Initialization method
        /// </summary>
        void Initialize();

        /// <summary>
        /// Starts the voice recording service
        /// </summary>
        void StartVoiceRecording();

        /// <summary>
        /// Stops the voice recording service
        /// </summary>
        void StopVoiceRecording();

    }
}