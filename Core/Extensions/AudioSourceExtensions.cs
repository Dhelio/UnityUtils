using UnityEngine;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for <see cref="AudioSource"/>
    /// </summary>
    public static class AudioSourceExtensions {
        
        /// <summary>
        /// Calculates the decibels coming from this <see cref="AudioSource"/>
        /// </summary>
        /// <param name="SamplesToEvaluate">How many samples to evaluate to calculate the decibels. Must be a power of two.</param>
        /// <param name="Offset">Offset in the track.</param>
        /// <returns>The decibels of this <see cref="AudioSource"/> playing <see cref="AudioClip"/>.</returns>
        /// <exception cref="System.ArgumentException">Thrown if the parameter SamplesToEvaluate isn't a power of two.</exception>
        public static float Decibels(this AudioSource audioSource, int SamplesToEvaluate = 128, int Offset = 0) {
            if ((SamplesToEvaluate % 2) != 0) {
                throw new System.ArgumentException($"Parameter {SamplesToEvaluate} must be a power of two!");
            }
            float[] buffer = new float[SamplesToEvaluate];
            var clip = audioSource.clip;
            var length = clip.length;
            clip.GetData(buffer,Offset);

            // sum of squares
            float sos = 0f;
            float val;

            if (length > buffer.Length) {
                length = buffer.Length;
            }

            for (int i = 0; i < length; i++) {
                val = buffer[Offset];
                sos += val * val;
            }

            // sqrt of average
            var rms = Mathf.Sqrt(sos / length);
            return 10 * Mathf.Log10(rms);
        }
    }

}