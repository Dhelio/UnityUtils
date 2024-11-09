using System.Globalization;
using UnityEngine;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extensions for the float numbers
    /// </summary>
    public static class FloatExtensions {

        /// <summary>
        /// Checks if the given value is between the defined min max values
        /// </summary>
        /// <returns>True if it's between Min and Max, False otherwise</returns>
        public static bool IsBetween(this float target, float min, float max) {
            
            return (target > min && target < max);
        }

        /// <summary>
        /// Checks if the given value is between the defined min max values. If the min is greater than max, then check if it's either greater than min or lesser than max, thus wrapping the value between different ranges.
        /// e.g. with a value of 4
        /// min = 3, max = 5 => returns true
        /// min = 5, max = 3 => returns false
        /// min = 2, max = 1 => returns true
        /// </summary>
        public static bool IsEllipticallyBetween(this float target, float min, float max) {
            return (min < max) ? IsBetween(target, min, max) : (target > min || target < max);
        }

        /// <summary>
        /// Normalizes the value of the float between a min and a max
        /// </summary>
        public static float Normalized(this float target, float min, float max) {
            return (target-min)/(max-min);
        }

        /// <summary>
        /// Converts this float to string using an american notation (basically using a point as a less than zero digit)
        /// </summary>
        /// <returns>An american style float (e.g. for the float 1,54432 returns a string "1.54432"</returns>
        public static string ToStringAmericanStyle(this float target) {
            var cultureInfo = new CultureInfo("en-US");
            return target.ToString(cultureInfo);
        }

        /// <summary>
        /// Determines the cosine similarity as defined <see cref="https://en.wikipedia.org/wiki/Cosine_similarity">here</see>
        /// </summary>
        /// <param name="array">the array to compare this one with</param>
        /// <param name="shouldBeNormalized">Wheter the two arrays should be normalized prior to the comparison</param>
        /// <returns>A value between 0 and 1 indicatting the similarity between these arrays</returns>
        public static float CosineSimilarity(this float[] target, float[] array, bool shouldBeNormalized = false) {

            // dot product of the two comparing vectors
            float dotProduct = 0;
            for (int i = 0; i < target.Length; i++)
                dotProduct += target[i] * array[i];

            //lenghts and optional normalization of the vectors
            float magnitudeA = 0;
            float magnitudeB = 0;

            if (!shouldBeNormalized) {
                magnitudeA = 1;
                magnitudeB = 1;
            } else {
                for (int i = 0; i < target.Length; i++) {
                    magnitudeA += Mathf.Pow(target[i], 2.0f);
                    magnitudeB += Mathf.Pow(array[i], 2.0f);
                }

                magnitudeA = Mathf.Sqrt(magnitudeA);
                magnitudeB = Mathf.Sqrt(magnitudeB);
            }

            // cosine similarity
            float cosineSimilarity = dotProduct / (magnitudeA * magnitudeB);

            return cosineSimilarity;
        }
    }
}