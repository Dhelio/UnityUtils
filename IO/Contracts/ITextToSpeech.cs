using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Castrimaris.IO.Contracts {

    /// <summary>
    /// Interface implemented by text to speech services
    /// </summary>
    public interface ITextToSpeech {

        public bool IsSpeaking { get; }
        public bool CanSpeak { get; set; }
        public UnityEvent OnStartedSpeaking { get; }
        public UnityEvent OnStoppedSpeaking { get; }

        /// <summary>
        /// Makes the text to speech say something.
        /// </summary>
        /// <param name="Text">The text to say</param>
        public void Speak(string Text);

        /// <summary>
        /// Makes the text to speech say something. If there's other text being spoke, it is enqueued right after
        /// </summary>
        /// <param name="Text">The text to say</param>
        public void SpeakEnqueue(string Text);

        
        public Task<AudioClip> Generate(string Text);

        public Task<AudioClip> GenerateAsync(string Text);
    }

}
