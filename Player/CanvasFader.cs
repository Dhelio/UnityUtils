using DG.Tweening;
using Castrimaris.Player.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace Castrimaris.Player {

    [RequireComponent(typeof(Image))]
    public class CanvasFader : MonoBehaviour, IFader {

        [Header("Parameters")]
        [SerializeField, Range(.5f, 2f)] private float fadeDuration = 1.0f;

        private Image image;

        public void FadeIn() {
            image.DOFade(1.0f, fadeDuration);
        }

        public void FadeOut() {
            image.DOFade(0.0f, fadeDuration);
        }

        private void Awake() {
            image = GetComponent<Image>();
        }
    }

}