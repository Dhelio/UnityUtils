using UnityEngine.Events;

namespace Castrimaris.IO.Contracts {

    /// <summary>
    /// Interface implemented by AI assistants.
    /// </summary>
    public interface IAssistant {
        public bool IsThinking { get; }

        public UnityEvent<string> OnChatResponse { get; }

        /// <summary>
        /// Prompts the assistant for a result.
        /// </summary>
        /// <param name="Text">Something to ask to the assistant.</param>
        public void Ask(string Text);
    }

}