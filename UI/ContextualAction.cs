using Castrimaris.Core;
using Castrimaris.UI.Contracts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Castrimaris.UI {

    /// <summary>
    /// A UI element that displays a contextual action for 3D objects, showing what needs to be press ot interect with it.
    /// </summary>
    public class ContextualAction : MonoBehaviour, IContextualAction {

        [Header("References")]
        [SerializeField] private InterfaceReference<ISubtitledImage> promptImage;
        [SerializeField] private InterfaceReference<IText> promptDescription;

        public void ResetProgress() {
            throw new System.NotImplementedException();
        }

        public void SetActionDescription(string text) => promptDescription.Interface.Text = text;

        public void SetActionImage(Texture2D texture) {
            promptImage.Interface.Image = texture;
        }

        public void SetPosition(Vector3 worldPosition) {
            var screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            this.transform.position = screenPosition;
        }

        public void SetProgress(float progress) {
            throw new System.NotImplementedException();
        }

        private void Reset() {
            var iSubtitledImage = GetComponentInChildren<ISubtitledImage>();
            promptImage = new InterfaceReference<ISubtitledImage>(iSubtitledImage.gameObject);
            var iText = GetComponentsInChildren<IText>().Where(txt => txt is not ISubtitledImage).FirstOrDefault();
            promptDescription = new InterfaceReference<IText>(iText.gameObject);
        }
    }
}
