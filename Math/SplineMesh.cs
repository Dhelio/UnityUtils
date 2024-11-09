using SplineMesh;
using UnityEngine;

namespace Castrimaris.Math {
    /// <summary>
    /// Spline inherited class that implements the ISplineMesh interface too for ease of use.
    /// </summary>
    public class SplineMesh : Spline, ISplineMesh {

        #region PRIVATE METHODS

        /// <summary>
        /// Takes the distance on the spline and wraps it on the spline by "looping" the spline if necessary
        /// </summary>
        /// <param name="DistanceOnTheSpline">The distance</param>
        private void WrapSplineDistance(ref float DistanceOnTheSpline) {
            if (DistanceOnTheSpline > Length) {
                do {
                    DistanceOnTheSpline -= Length;
                } while (DistanceOnTheSpline > Length);
            } else if (DistanceOnTheSpline < 0) {
                do {
                    DistanceOnTheSpline += Length;
                } while (DistanceOnTheSpline < 0);
            }
        }

        #endregion

        #region PUBLIC METHODS

        public Vector3 GetLocalPosition(float DistanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;

            if (IsLoop)
                WrapSplineDistance(ref DistanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(DistanceOnTheSpline);
            position = sample.location;

            return position;
        }

        public Quaternion GetLocalRotation(float DistanceOnTheSpline) {
            Quaternion rotation = Quaternion.identity;

            if (IsLoop)
                WrapSplineDistance(ref DistanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(DistanceOnTheSpline);
            rotation = sample.Rotation;

            return rotation;
        }

        public (Vector3, Quaternion) GetLocalPositionAndRotation(float DistanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;
            Quaternion rotation = Quaternion.identity;

            if (IsLoop)
                WrapSplineDistance(ref DistanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(DistanceOnTheSpline);
            position = sample.location;
            rotation = sample.Rotation;

            return (position, rotation);
        }

        public float GetSplineLength() {
            return Length;
        }

        public Vector3 GetWorldPosition(float DistanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;

            if (IsLoop)
                WrapSplineDistance(ref DistanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(DistanceOnTheSpline);
            position = this.transform.TransformPoint(sample.location);

            return position;
        }

        public Quaternion GetWorldRotation(float DistanceOnTheSpline) {
            return GetLocalRotation(DistanceOnTheSpline);
        }

        public (Vector3, Quaternion) GetWorldPositionAndRotation(float DistanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;
            Quaternion rotation = Quaternion.identity;

            if (IsLoop)
                WrapSplineDistance(ref DistanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(DistanceOnTheSpline);
            position = this.transform.TransformPoint(sample.location);
            rotation = sample.Rotation;

            return (position, rotation);
        }

        #endregion
    }
}
