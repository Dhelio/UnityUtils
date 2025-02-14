
using Castrimaris.Core;
using UnityEngine;

namespace Castrimaris.UI.Contracts {

    /// <summary>
    /// Contract for UI elements that show a contextual action to the user.
    /// </summary>
    public interface IContextualAction : IGameObjectSource {

        public void SetActionDescription(string text);
        public void SetActionImage(Texture2D texture);
        public void SetPosition(Vector3 worldPosition);
        public void SetProgress(float progress);
        public void ResetProgress();

    }
}