using UnityEngine;

namespace Castrimaris.Math {
    public interface ISplineMesh {

        /// <summary>
        /// Gets the local position at the specified length on the spline
        /// </summary>
        /// <param name="DistanceOnTheSpline">The length on the spline, between 0 and the spline length in world units.</param>
        /// <returns></returns>
        public Vector3 GetLocalPosition(float DistanceOnTheSpline);

        /// <summary>
        /// Gets the local rotation at the specified length on the spline
        /// </summary>
        /// <param name="DistanceOnTheSpline">The length on the spline, between 0 and the spline length in world units.</param>
        /// <returns>The local rotation</returns>
        public Quaternion GetLocalRotation(float DistanceOnTheSpline);

        /// <summary>
        /// Gets the local position and rotation at the specified length on the spline
        /// </summary>
        /// <param name="DistanceOnTheSpline">The length on the spline, between 0 and the spline length in world units.</param>
        /// <returns>A Vector3 and quaternion tuple representing local position and rotation</returns>
        public (Vector3, Quaternion) GetLocalPositionAndRotation(float DistanceOnTheSpline);

        /// <summary>
        /// Gets the world position at the specified length on the spline
        /// </summary>
        /// <param name="DistanceOnTheSpline">The length on the spline, between 0 and the spline length in world units.</param>
        /// <returns>The world position</returns>
        public Vector3 GetWorldPosition(float DistanceOnTheSpline);

        /// <summary>
        /// Gets the rotation at the specified length on the spline
        /// </summary>
        /// <param name="DistanceOnTheSpline">The length on the spline, between 0 and the spline length in world units.</param>
        /// <returns>The rotation</returns>
        public Quaternion GetWorldRotation(float DistanceOnTheSpline);

        /// <summary>
        /// Gets the world position and rotation at the specified length on the spline
        /// </summary>
        /// <param name="DistanceOnTheSpline">The length on the spline, between 0 and the spline length in world units.</param>
        /// <returns>A Vecto3 and quaternion tuple representing world position and rotation</returns>
        public (Vector3, Quaternion) GetWorldPositionAndRotation(float DistanceOnTheSpline);

        /// <summary>
        /// Gets the spline length in world units
        /// </summary>
        public float GetSplineLength();
    }
}
