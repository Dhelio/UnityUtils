using Castrimaris.Core;

namespace Castrimaris.UI.Contracts {
    /// <summary>
    /// Contract for objects that can fade in and out, such as fullscreen fade to black/white transitions, etc.
    /// </summary>
    public interface IFader :IGameObjectSource {

        /// <summary>
        /// Executes a fade in animation.
        /// </summary>
        public void FadeIn();

        /// <summary>
        /// Executes a fade out animation
        /// </summary>
        public void FadeOut();
    }
}