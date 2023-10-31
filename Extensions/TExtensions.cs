using UnityEngine;

namespace Castrimaris.Core {

    public static class TExtensions {

        /// <summary>
        /// Performs a Sanity Check on this component. If it's null, then it tries to find it with a
        /// GameObject.Find(); if that is null too, throws NullReference exception. Best used in Awake().
        /// </summary>
        /// <param name="GameObjectName">The name of the object to look for in case it is null.</param>
        /// <exception cref="System.NullReferenceException">
        /// The searched reference doesn't exist.
        /// </exception>
        public static void SanityCheck<T>(this T target, string GameObjectName) {
            if (target == null) {
                target = GameObject.Find(GameObjectName).GetComponent<T>();
                if (target == null) {
                    throw new System.NullReferenceException($"Could not find a reference for {GameObjectName}. Did you forget to assign it in the Editor?");
                }
            }
        }

        /// <summary>
        /// Looks for a specific component in this transform parents recursively.
        /// </summary>
        /// <typeparam name="T">Type of the Component to look for.</typeparam>
        /// <param name="target">The transform on which to perform the search</param>
        /// <returns>The component if found, null otherwise</returns>
        public static T GetComponentInParentRecursive<T>(this Component target) where T : Component {
            var result = target.GetComponentInParent<T>();
            if (result == null && target.transform.parent != null) {
                return target.transform.parent.GetComponentInParentRecursive<T>();
            }
            return result;
        }

        /// <summary>
        /// Looks for a specific inteface in this component parents recursively.
        /// </summary>
        /// <typeparam name="T">An interface</typeparam>
        /// <param name="target">The component where to perform the recursive search.</param>
        /// <returns>The interface if found, false otherwise</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the passed type is not an interface.</exception>
        public static T GetInterfaceInParentRecursive<T>(this Component target) where T : class {
            //Type check
            if (!typeof(T).IsInterface) {
                throw new System.InvalidOperationException($"Could not retrieve interface for type {typeof(T)}, as it is not an interface.");
            }

            //Local function that implements the recursion behaviour
            T GetInterfaceInParentRecursively(Component component) {
                var result = target.GetComponentInParent<T>();
                if (result == null && target.transform.parent != null) {
                    return GetInterfaceInParentRecursively(target.transform.parent);
                }
                return result;
            }

            return GetInterfaceInParentRecursively(target);
        }

    }
}
