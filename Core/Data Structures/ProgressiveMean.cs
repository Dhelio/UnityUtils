using Castrimaris.Core.Extensions;

namespace Castrimaris.Core {
    public class ProgressiveMean {
        private float mean;
        private float variance;
        private int count;
        private readonly float standardVarianceTolerance;

        public float Mean => mean;

        public ProgressiveMean(float standardVarianceTolerance = 2.0f) {
            this.standardVarianceTolerance = standardVarianceTolerance;
            count = 0;
            variance = 0;
            mean = 0;
        }

        public void Add(float value) {
            count++;
            if (count == 1) {
                mean = value;
                variance = 0.0f;
                return;
            }

            var standardDeviation = (float)System.Math.Sqrt(variance);
            var distanceFromMean = System.Math.Abs(value - mean);
            var maxToleratedDistance = standardVarianceTolerance * standardDeviation;
            var weight = (maxToleratedDistance > 0) ? System.Math.Max(.01f, 1.0f - (distanceFromMean / maxToleratedDistance)) : 1.0f;
            mean = (weight * value + (count - 1) * mean) / (count - 1 + weight);
            variance = ((count-2) * variance + weight * (value - mean) * (value - mean)) / (count -1);
        }

        public void AddRange(float[] values) => values.ForEach(value => Add(value));
    }
}
