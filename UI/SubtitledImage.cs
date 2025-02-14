using Castrimaris.Core.Exceptions;
using Castrimaris.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace Castrimaris.UI {

    /// <summary>
    /// Simple component that shows a <see cref="UnityEngine.UI.Image"/> component together with a text.
    /// </summary>
    public class SubtitledImage : MonoBehaviour, ISubtitledImage {

        private IText text;
        private Image image;

        public Texture2D Image { get => image.sprite.texture; set => Sprite.Create(value, image.sprite.rect, image.sprite.pivot); }
        public string Text { get => text.Text; set => text.Text = value; }

        public Texture2D GetImage() => Image;
        public string GetText() => Text;
        public void SetImage(Texture2D image) => Image = image;
        public void SetText(string text) => Text = text;
        
        private void Reset() {
            text = GetComponentInChildren<IText>();
            image = GetComponentInChildren<Image>();

            if (text == null && image == null)
                throw new ReferenceMissingException($"{nameof(text)} and {nameof(image)}");
            if (text == null)
                throw new ReferenceMissingException(nameof(text));
            if (image == null)
                throw new ReferenceMissingException(nameof(image));
        }

        private void Awake() {
            text ??= GetComponentInChildren<IText>() ?? throw new ReferenceMissingException(nameof(text));
            image ??= GetComponentInChildren<Image>() ?? throw new ReferenceMissingException(nameof(image));
        }
    }

}