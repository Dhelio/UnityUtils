using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extensions for the double numbers
    /// </summary>
    public static class DoubleExtensions {

        /// <summary>
        /// Converts this double to string using an american notation (basically using a point as a less than zero digit)
        /// </summary>
        /// <returns>An american style double (e.g. for the double 1,54432 returns a string "1.54432"</returns>
        public static string ToAmericanStyle(this double target) {
            var cultureInfo = new CultureInfo("en-US");
            return target.ToString(cultureInfo);
        }

        /// <summary>
        /// Determines the cosine similarity as defined <see cref="https://en.wikipedia.org/wiki/Cosine_similarity">here</see>
        /// </summary>
        /// <param name="comparer">the array to compare this one with</param>
        /// <param name="shouldBeNormalized">Wheter the two arrays should be normalized prior to the comparison</param>
        /// <returns>A value between 0 and 1 indicatting the similarity between these arrays</returns>
        public static double CosineSimilarity(this double[] target, double[] comparer, bool shouldBeNormalized = false) {
            // dot product of the two comparing vectors
            double dotProduct = 0;
            for (int i = 0; i < target.Length; i++)
                dotProduct += target[i] * comparer[i];

            //lenghts and optional normalization of the vectors
            double magnitudeA = 0;
            double magnitudeB = 0;

            if (!shouldBeNormalized) {
                magnitudeA = 1;
                magnitudeB = 1;
            } else {
                for (int i = 0; i < target.Length; i++) {

                    magnitudeA += System.Math.Pow(target[i], 2.0f);
                    magnitudeB += System.Math.Pow(comparer[i], 2.0f);
                }

                magnitudeA = System.Math.Sqrt(magnitudeA);
                magnitudeB = System.Math.Sqrt(magnitudeB);
            }

            // cosine similarity
            double cosineSimilarity = dotProduct / (magnitudeA * magnitudeB);

            return cosineSimilarity;
        }

        /// <inheritdoc cref="CosineSimilarity(double[], double[], bool)"/>
        public static double CosineSimilarity(this double[] target, List<double> comparer, bool shouldBeNormalized = false) => CosineSimilarity(target, comparer.ToArray(), shouldBeNormalized);

        /// <inheritdoc cref="CosineSimilarity(double[], double[], bool)"/>
        public static double CosineSimilarity(this List<double> target, List<double> comparer, bool shouldBeNormalized = false) => CosineSimilarity(target.ToArray(), comparer.ToArray(), shouldBeNormalized);

        /// <inheritdoc cref="CosineSimilarity(double[], double[], bool)"/>
        public static double CosineSimilarity(this List<double> target, double[] comparer, bool shouldBeNormalized = false) => CosineSimilarity(target.ToArray(), comparer, shouldBeNormalized);
    }

}