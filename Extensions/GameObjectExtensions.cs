using UnityEngine;

namespace Castrimaris.Core {
    /// <summary>
    /// Extensions for the GameObject class
    /// </summary>
    public static class GameObjectExtensions {

        /// <summary>
        /// Searches for a component in this GameObject or its children
        /// </summary>
        /// <typeparam name="T">Type of the component to search for.</typeparam>
        /// <param name="target">The object on which to search</param>
        /// <returns>The component if found, null otherwise.</returns>
        public static T FindComponent<T>(this GameObject target) {
            T result = default;
            try {
                result = target.GetComponent<T>();
            } catch (System.Exception _) {
                result = target.GetComponentInChildren<T>();
            } finally {
                if (result == null) {
                    result = target.GetComponentInChildren<T>();
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if this <see cref="GameObject"/> has the indicated component
        /// </summary>
        /// <returns>True if it has the component, False otherwise</returns>
        public static bool HasComponent<T>(this GameObject target) {
            try {
                if (target.GetComponent<T>() == null) {
                    return false;
                } else {
                    return true;
                }
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Checks if this <see cref="GameObject"/> has a <see cref="Tags"/> component and compares tags.
        /// </summary>
        /// <param name="target">The target <see cref="GameObject"/></param>
        /// <param name="Tags">Tags to check against.</param>
        /// <returns>True if the object has all tags.</returns>
        public static bool HasTags(this GameObject target, params string[] Tags) {
            if (!HasComponent<Tags>(target))
                return false;

            var tags = target.GetComponent<Tags>();
            return tags.Has(Tags);
        }

        /// <summary>
        /// Tries to get the component from this <see cref="GameObject"/>, otherwise tries to retrieve it from its children.
        /// </summary>
        /// <typeparam name="T">Type of the component to look for.</typeparam>
        /// <param name="target">The <see cref="GameObject"/> where to look for the component.</param>
        /// <returns>The component or exception.</returns>
        /// <exception cref="MissingComponentException">Thrown if no component of this type is in this <see cref="GameObject"/> or its children.</exception>
        public static T RecursiveTryGetComponent<T>(this GameObject target) where T: Component {
            T component;
            if (!target.TryGetComponent<T>(out component)) {
                component = target.GetComponentInChildren<T>();
                if (component == null) {
                    throw new MissingComponentException($"No component of type {nameof(T)} attached to the GameObject {target.name}!");
                }
            }
            return component;
        }

    }
}
