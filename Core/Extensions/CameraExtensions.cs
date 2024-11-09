using UnityEngine;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for <see cref="Camera"/>
    /// </summary>
    public static class CameraExtensions {

        /// <summary>
        /// Determines target position and rotation to apply to the perspective <see cref="Camera"/> in order to fit the given <see cref="Bounds"/> into the field of view
        /// </summary>
        /// <param name="target">The <see cref="Camera"/> for which to determine target transformations</param>
        /// <param name="targetPosition">The resulting target position</param>
        /// <param name="targetRotation">The resulting target orientation</param>
        /// <param name="bounds">The <see cref="Bounds"/> to fit into the view of the <see cref="Camera"/></param>
        /// <param name="additionalSpacing">Additional spacing factor (uses <code>(1 + spacing) * boundsSize</code>) to leave free around the given <see cref="Bounds"/></param>
        public static void GetPerspectiveFocusTransforms(this Camera target, out Vector3 targetPosition, out Quaternion targetRotation, Bounds bounds, float additionalSpacing = 0f) {
            var objectSizes = bounds.size;
            var objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);

            // Visible height 1 meter in front
            var cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * target.fieldOfView);

            // Combined wanted distance from the object
            var distance = (1 + additionalSpacing) * objectSize / cameraView;

            // Estimated offset from the center to the outside of the object
            distance += 0.5f * objectSize;

            targetPosition = bounds.center - distance * target.transform.forward;
            targetRotation = Quaternion.LookRotation(bounds.center - targetPosition);

            var maxExtent = bounds.extents.magnitude;
            var minDistance = maxExtent * (1 + additionalSpacing) / Mathf.Tan(0.5f * Mathf.Deg2Rad * target.fieldOfView);

            targetPosition = bounds.center - target.transform.forward * minDistance;

            // camera.nearClipPlane = minDistance - maxExtent;
            targetRotation = Quaternion.LookRotation(bounds.center - targetPosition);
        }

        /// <summary>
        /// Determines target position and size to apply to the orthographic <see cref="Camera"/> in order to fit the given <see cref="Bounds"/> into the field of view
        /// </summary>
        /// <param name="target">The <see cref="Camera"/> for which to determine target transformations</param>
        /// <param name="targetPosition">The resulting target position</param>
        /// <param name="targetOrthographicSize">The resulting target size</param>
        /// <param name="bounds">The <see cref="Bounds"/> to fit into the view of the <see cref="Camera"/></param>
        /// <param name="additionalSpacing">Additional spacing factor (uses <code>(1 + spacing) * boundsSize</code>) to leave free around the given <see cref="Bounds"/></param>
        public static void GetOrthographicFocusTransforms(this Camera target, out Vector3 targetPosition, out float targetOrthographicSize, Bounds bounds, float additionalSpacing = 0f) {
            var size = bounds.size;
            var factor = 1 + additionalSpacing;

            var width = size.x * factor;
            var height = size.y * factor;

            if (width > height) {
                targetOrthographicSize = Mathf.Abs(width) / target.aspect / 2f;
            } else {
                targetOrthographicSize = Mathf.Abs(height) / 2f;
            }

            targetPosition = bounds.center;

            targetPosition -= Vector3.Project(targetPosition - target.transform.position, target.transform.forward);

            targetOrthographicSize = Mathf.Max(targetOrthographicSize, Mathf.Epsilon);
        }

        /// <summary>
        /// Wraps the <see cref="Camera"/> FoV around the <see cref="Bounds"/>, with an optional padding.
        /// </summary>
        /// <param name="target">The <see cref="Camera"/> for which to apply the wrapping</param>
        /// <param name="bounds">The <see cref="Bounds"/> to fit into the view of the <see cref="Camera"/></param>
        /// <param name="additionalSpacing">Additional spacing factor (uses <code>(1 + spacing) * boundsSize</code>) to leave free around the given <see cref="Bounds"/></param>
        public static void WrapToBounds(this Camera target, Bounds bounds, float additionalSpacing = 0.0f) {
            if (target.orthographic) {
                target.GetOrthographicFocusTransforms(out var targetPosition, out var targetOrthographicSize, bounds, additionalSpacing);
                target.transform.position = targetPosition;
                target.orthographicSize = targetOrthographicSize;
            } else {
                target.GetPerspectiveFocusTransforms(out var targetPosition, out var targetRotation, bounds, additionalSpacing);
                target.transform.position = targetPosition;
                target.transform.rotation = targetRotation;
            }
        }
    }


}
