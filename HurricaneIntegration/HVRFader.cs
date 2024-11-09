using Castrimaris.Player.Contracts;
using HurricaneVR.Framework.Core.Player;
using UnityEngine;

namespace Castrimaris.HurricaneIntegration {

    [RequireComponent(typeof(HVRCanvasFade))]
    public class HVRFader : MonoBehaviour, IFader {

        [Header("Parameters")]
        [SerializeField, Range(.5f, 2f)] private float fadeTime = 1.0f;

        private HVRCanvasFade canvasFade;

        public void FadeIn() => canvasFade.Fade(0, fadeTime);
        public void FadeOut() => canvasFade.Fade(1, fadeTime);

        private void Awake() {
            canvasFade = GetComponent<HVRCanvasFade>();
        }
    }

}