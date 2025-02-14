#if DOTWEEN

using DG.Tweening;
using Castrimaris.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace Castrimaris.Player {

    /// <summary>
    /// A simple fader that uses a standard UI Image.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ImageFader : MonoBehaviour, IFader {

        [Header("Parameters")]
        [SerializeField, Range(.5f, 2f)] private float fadeDuration = 1.0f;

        private Image image;

        public void FadeIn() {
            image.DOKill();
            image.DOFade(1.0f, fadeDuration);
        }

        public void FadeOut() {
            image.DOKill();
            image.DOFade(0.0f, fadeDuration);
        }

        private void Awake() {
            image = GetComponent<Image>();
        }
    }

}

#endif