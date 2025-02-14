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
        /// <param name="distanceOnTheSpline">The distance</param>
        private void WrapSplineDistance(ref float distanceOnTheSpline) {
            if (distanceOnTheSpline > Length) {
                do {
                    distanceOnTheSpline -= Length;
                } while (distanceOnTheSpline > Length);
            } else if (distanceOnTheSpline < 0) {
                do {
                    distanceOnTheSpline += Length;
                } while (distanceOnTheSpline < 0);
            }
        }

        #endregion

        #region PUBLIC METHODS

        public Vector3 GetLocalPosition(float distanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;

            if (IsLoop)
                WrapSplineDistance(ref distanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(distanceOnTheSpline);
            position = sample.location;

            return position;
        }

        public Quaternion GetLocalRotation(float distanceOnTheSpline) {
            Quaternion rotation = Quaternion.identity;

            if (IsLoop)
                WrapSplineDistance(ref distanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(distanceOnTheSpline);
            rotation = sample.Rotation;

            return rotation;
        }

        public (Vector3, Quaternion) GetLocalPositionAndRotation(float distanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;
            Quaternion rotation = Quaternion.identity;

            if (IsLoop)
                WrapSplineDistance(ref distanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(distanceOnTheSpline);
            position = sample.location;
            rotation = sample.Rotation;

            return (position, rotation);
        }

        public float GetSplineLength() {
            return Length;
        }

        public Vector3 GetWorldPosition(float distanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;

            if (IsLoop)
                WrapSplineDistance(ref distanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(distanceOnTheSpline);
            position = this.transform.TransformPoint(sample.location);

            return position;
        }

        public Quaternion GetWorldRotation(float distanceOnTheSpline) {
            return GetLocalRotation(distanceOnTheSpline);
        }

        public (Vector3, Quaternion) GetWorldPositionAndRotation(float distanceOnTheSpline) {
            Vector3 position = Vector3.positiveInfinity;
            Quaternion rotation = Quaternion.identity;

            if (IsLoop)
                WrapSplineDistance(ref distanceOnTheSpline);

            CurveSample sample = GetSampleAtDistance(distanceOnTheSpline);
            position = this.transform.TransformPoint(sample.location);
            rotation = sample.Rotation;

            return (position, rotation);
        }

        #endregion
    }
}
