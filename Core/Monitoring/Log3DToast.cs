using Castrimaris.Core.Monitoring;
using System.Collections;
using UnityEngine;

namespace Castrimaris.Core {

    /// <summary>
    /// Logs messages in the 3D space and shows them only for a short while.
    /// </summary>
    public class Log3DToast : Log3D {

        [Header("Parameters")]
        [SerializeField] private float toastDuration = 2.0f;

        private Coroutine coroutine = null;

        public override void Append(string Message) {
            console.text = Message;
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine(ShowToast());
        }

        private IEnumerator ShowToast() {
            console.color = Color.white;
            yield return new WaitForSeconds(toastDuration);

            var stepValue = .05f;
            var wfs = new WaitForSeconds(stepValue);
            var step = 1 / stepValue;
            int multiplier = 0;
            var originalColor = console.color;
            var targetColor = new Color(1,1,1,0);
            do {
                console.color = Color.Lerp(originalColor, targetColor, 1 - (step * multiplier));
                yield return wfs;
                multiplier++;
            } while (console.color.a > 0);
        }

    }

}
