using TMPro;
using UnityEngine;

namespace Castrimaris.Monitoring {

    /// <summary>
    /// Simple FPS counter
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class FPSCounter : MonoBehaviour {

        [SerializeField, Range(.1f,2f)] private float sampleDuration = 1.0f;

        private TextMeshPro label;
        private float totalFrames = 0;
        private float totalInstantFrames = 0;
        private float bestInstantFrames = float.MaxValue;
        private float worstInstantFrames = 0;

        private void Awake() {
            label = GetComponent<TextMeshPro>();
        }

        private void Update() {
            float instantFrames = Time.unscaledDeltaTime;
            totalFrames++;
            totalInstantFrames += instantFrames;
            if (instantFrames < bestInstantFrames) {
                bestInstantFrames = instantFrames;
            }
            if (instantFrames > worstInstantFrames) {
                worstInstantFrames = instantFrames;
            }
            if (totalInstantFrames >= sampleDuration) {
                float averageFrames = totalFrames / totalInstantFrames;
                label.SetText("AVG: {0:0}\nHIG: {1:0}\nLOW: {2:0}", averageFrames, bestInstantFrames, worstInstantFrames);
                totalFrames = 0;
                totalInstantFrames = 0;
                bestInstantFrames = 0;
                worstInstantFrames = 0;
            }
        }
    }

} 