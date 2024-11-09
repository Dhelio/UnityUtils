using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Castrimaris.Core.Extensions {

    /// <summary>
    /// Extension methods for <see cref="Transform"/>
    /// </summary>
    public static class TransformExtensions {

        /// <summary>
        /// Looks for a child <see cref="Transform"/> recursively.
        /// </summary>
        /// <param name="Name">The name of the child <see cref="Transform"/>. Can be any nested child of this <see cref="Transform"/></param>
        /// <returns>The child <see cref="Transform"/> if found, null otherwise.</returns>
        public static Transform RecursiveFind(this Transform target, string Name) {
            foreach (Transform childTransform in target) {
                if (childTransform.name == Name) {
                    return childTransform;
                } else {
                    Transform found = RecursiveFind(childTransform, Name);
                    if (found != null) {
                        return found;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Looks for all certain childs in a <see cref="Transform"/> recursively.
        /// </summary>
        /// <param name="name">The name that the childs must share</param>
        /// <returns>The array of all the childs with the same name, empty array otherwise.</returns>
        public static Transform[] MultipleRecursiveFind(this Transform target, string name) {
            var allTransforms = target.GetComponentsInChildren<Transform>();

            var result = (from t in allTransforms
                          where t.name == name
                          select t).ToArray();

            return result.ToArray();
        }

        /// <summary>
        /// Looks for a parent <see cref="Transform"/> recursively.
        /// </summary>
        /// <param name="Name">The name of the parent transform to look for. Can be any nested parent of this <see cref="Transform"/>.</param>
        /// <returns>The parent <see cref="Transform"/> if found, null otherwise</returns>
        public static Transform RecursiveFindParent(this Transform target, string Name) {
            var name = target.parent.name;
            if (target.parent.name == Name) {
                return target.parent;
            } else if (target.parent != null) {
                return target.parent.RecursiveFindParent(Name);
            }
            return null;
        }


        /// <summary>
        /// Looks for multiple childs under this <see cref="Transform"/>.
        /// </summary>
        /// <param name="Exact">True if the search should stop when it has found exactly the number passed names, false if it should look for more same named children.</param>
        /// <param name="Names">Names of the childs to search</param>
        /// <returns></returns>
        public static Transform[] MultipleFind(this Transform target, bool Exact, params string[] Names) {
            List<Transform> result = new List<Transform>();
            List<string> names = Names.ToList();
            for (int i = 0; i < target.childCount; i++) {
                if (Names.Any(target.GetChild(i).name.Contains)) {
                    result.Add(target.GetChild(i));
                    if (Exact) {
                        if (names.Count == result.Count)
                            break;
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Destroys all childs of this <see cref="Transform"/> immediately. You are strogly recommended to use DestroyChilds instead.
        /// </summary>
        /// <returns>The number of destroyed childs.</returns>
        public static int DestroyChildsImmediate(this Transform target) {
            int i;
            for (i = 0; target.childCount > 0; i++) {
                GameObject.DestroyImmediate(target.GetChild(0).gameObject);
            }
            return i + 1;
        }

        /// <summary>
        /// Destroys all childs of this <see cref="Transform"/>.
        /// </summary>
        /// <returns>The number of destroyed childs.</returns>
        public static int DestroyChilds(this Transform target) {
            int i;
            for (i = 0; i < target.childCount; i++) {
                GameObject.Destroy(target.GetChild(i).gameObject);
            }
            return i + 1;
        }

        /// <summary>
        /// Sets all children of this <see cref="Transform"/> to the indicated active state.
        /// </summary>
        /// <param name="ChildrenActiveState">True if all childs should be active, false otherwise</param>
        public static void SetChildrenActiveState(this Transform target, bool ChildrenActiveState) {
            for (int i = 0; i < target.childCount; i++) {
                target.GetChild(i).gameObject.SetActive(ChildrenActiveState);
            }
        }

        /// <summary>
        /// Shuffles children order randomly.
        /// </summary>
        public static void ShuffleChildren(this Transform target) {
            System.Random rng = new System.Random();
            for (int i = 0; i < target.childCount; i++) {
                target.GetChild(i).SetSiblingIndex(rng.Next(0, target.childCount - 1));
            }
        }

        /// <summary>
        /// Checks if this <see cref="Transform"/> has a <see cref="Tags"/> component and compares tags.
        /// </summary>
        /// <param name="target">The target <see cref="Transform"/></param>
        /// <param name="Tags">Tags to check against.</param>
        /// <returns>True if the object has all tags.</returns>
        public static bool HasTags(this Transform target, params string[] Tags) {
            if (!target.gameObject.HasComponent<Tags>())
                return false;

            var tags = target.GetComponent<Tags>();
            return tags.Has(Tags);
        }

        [Obsolete("To be removed.")]
        public static Vector3 GetWorldPositionByRoot(this Transform transform, Vector3 position) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var positionInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(position);
            var positionInWorldSpace = totalLocalToWorldMatrix.MultiplyPoint3x4(positionInRootParentSpace);

            return positionInWorldSpace;
        }
        [Obsolete("To be removed.")]
        public static Vector3 GetWorldDirectionByRoot(this Transform transform, Vector3 hitInfoNormal) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var normalInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(hitInfoNormal);
            var normalInWorldSpace = totalLocalToWorldMatrix.MultiplyPoint3x4(normalInRootParentSpace);

            return normalInWorldSpace;
        }
        [Obsolete("To be removed.")]
        public static Quaternion GetWorldRotationByRoot(this Transform transform, Quaternion rotation) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var rotationInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(rotation.eulerAngles);
            var rotationInWorldSpace = totalLocalToWorldMatrix.MultiplyPoint3x4(rotationInRootParentSpace);

            return Quaternion.Euler(rotationInWorldSpace);
        }
        [Obsolete("To be removed.")]
        public static Vector3 GetWorldForwardByRoot(this Transform transform, Vector3 forward) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var forwardInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(forward);
            var forwardInWorldSpace = totalLocalToWorldMatrix.MultiplyPoint3x4(forwardInRootParentSpace);

            return forwardInWorldSpace;
        }
        [Obsolete("To be removed.")]
        public static Vector3 GetWorldUpByRoot(this Transform transform, Vector3 up) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var upInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(up);
            var upInWorldSpace = totalLocalToWorldMatrix.MultiplyPoint3x4(upInRootParentSpace);

            return upInWorldSpace;
        }
        [Obsolete("To be removed.")]
        public static Vector3 GetWorldRightByRoot(this Transform transform, Vector3 right) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var rightInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(right);
            var rightInWorldSpace = totalLocalToWorldMatrix.MultiplyPoint3x4(rightInRootParentSpace);

            return rightInWorldSpace;
        }
        [Obsolete("To be removed.")]
        public static Vector3 GetLocalPositionByRoot(this Transform transform, Vector3 position) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var positionInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(position);

            var positionInLocalSpace = totalLocalToWorldMatrix.inverse.MultiplyPoint3x4(positionInRootParentSpace);

            return positionInLocalSpace;
        }
        [Obsolete("To be removed.")]
        public static Quaternion GetLocalRotationByRoot(this Transform transform, Quaternion rotation) {
            var rootParent = transform.root;
            var totalLocalToWorldMatrix = CalculateTotalLocalToWorldMatrix(rootParent);
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var rotationInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(rotation.eulerAngles);
            var rotationInLocalSpace = totalLocalToWorldMatrix.inverse.MultiplyPoint3x4(rotationInRootParentSpace);

            return Quaternion.Euler(rotationInLocalSpace);
        }
        [Obsolete("To be removed.")]
        public static void SetWorldPositionByRoot(this Transform transform, Vector3 position) {
            var rootParent = transform.root;
            var totalWorldToLocalMatrix = CalculateTotalWorldToLocalMatrix(rootParent);

            var positionInRootParentSpace = totalWorldToLocalMatrix.MultiplyPoint3x4(position);
            var localPosition = transform.InverseTransformPoint(positionInRootParentSpace);

            transform.localPosition = localPosition;
        }
        [Obsolete("To be removed.")]
        public static void SetLocalPositionByThisParent(this Transform transform, Vector3 position) {
            var parentTransform = transform.parent;
            if (parentTransform != null) {
                var localPosition = parentTransform.InverseTransformPoint(position);
                transform.localPosition = localPosition;
            } else {
                transform.localPosition = position;
            }
        }
        [Obsolete("To be removed.")]
        public static Vector3 ProjectOntoPlane(Vector3 point, Vector3 planeNormal, Vector3 planePoint) {
            var toPoint = point - planePoint;
            var distanceToPlane = Vector3.Dot(toPoint, planeNormal);
            var projectedPosition = point - planeNormal * distanceToPlane;
            return projectedPosition;
        }
        [Obsolete("To be removed.")]
        public static float NormalizeVelocity(float currentVelocity, float minVelocity, float maxVelocity) {
            return (currentVelocity - minVelocity) / (maxVelocity - minVelocity);
        }
        [Obsolete("To be removed.")]
        private static Matrix4x4 CalculateTotalLocalToWorldMatrix(Transform transform) {
            Matrix4x4 matrix = Matrix4x4.identity;

            while (transform != null) {
                matrix = transform.localToWorldMatrix * matrix;
                transform = transform.parent;
            }

            return matrix;
        }
        [Obsolete("To be removed.")]
        private static Matrix4x4 CalculateTotalWorldToLocalMatrix(Transform transform) {
            Matrix4x4 matrix = Matrix4x4.identity;

            while (transform != null) {
                matrix = matrix * transform.worldToLocalMatrix;
                transform = transform.parent;
            }

            return matrix;
        }
        [Obsolete("To be removed.")]
        public static Vector3 GetWorldPosition(this Transform transform, Vector3 position = default) {
            transform.GetPositionAndRotation(out Vector3 pos, out Quaternion rotation);
            position = pos + rotation * Vector3.Scale(transform.InverseTransformPoint(position == default ? pos : position), transform.localScale);
            return position;
        }
        [Obsolete("To be removed.")]
        public static Quaternion GetWorldRotation(this Transform transform) {
            transform.GetPositionAndRotation(out Vector3 _, out Quaternion rotation);
            return transform.rotation * rotation;
        }
        [Obsolete("To be removed.")]
        public static Vector3 GetLocalPositionByThisParent(this Transform transform, Vector3 position) {
            return transform.position + transform.rotation * Vector3.Scale(transform.InverseTransformPoint(position), transform.localScale);
        }
        [Obsolete("To be removed.")]
        public static Vector3 TransformPointUnscaledRecursive(this Transform transform, Vector3 position) {
            Transform currentTransform = transform;

            while (currentTransform.parent != null) {
                position = currentTransform.position + currentTransform.TransformDirection(position);
                currentTransform = currentTransform.parent;
            }

            return position;
        }

    }
}