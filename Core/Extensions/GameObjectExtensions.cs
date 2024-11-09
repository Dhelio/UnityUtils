using Castrimaris.Core.Monitoring;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Castrimaris.Core.Extensions {
    /// <summary>
    /// Extensions for the <see cref="GameObject"/> class
    /// </summary>
    public static class GameObjectExtensions {

        /// <summary>
        /// Gets the full hierarchy path leading to this <see cref="GameObject"/>
        /// </summary>
        /// <param name="target">The target <see cref="GameObject"/> of which retrieve the path</param>
        /// <returns>The Hiearchy path leading to this <see cref="GameObject"/></returns>
        public static string GetPath(this GameObject target) {
            string result = target.name;
            var t = target.transform;
            while (t.parent != null) {
                t = t.parent;
                result = $"{t.name}/{result}";
            }
            return result;
        }

        /// <summary>
        /// Searches for a component in this <see cref="GameObject"/> OR its children.
        /// </summary>
        /// <typeparam name="T">Type of the component to search for.</typeparam>
        /// <param name="target">The object on which to perform search.</param>
        /// <returns>The component if found, null otherwise.</returns>
        public static T GetComponentRecursive<T>(this GameObject target) {
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
        /// Searches for components in this <see cref="GameObject"/> AND its children.
        /// </summary>
        /// <typeparam name="T">Type of the component to search for.</typeparam>
        /// <param name="target">The object on which to perform the search.</param>
        /// <returns>A list of all components of the indicated type. The list may be 0 length.</returns>
        public static List<T> GetComponentsRecursive<T>(this GameObject target) {
            var result = new List<T>();
            if (target.TryGetComponent<T>(out var component)) {
                result.Add(component);
            }
            var components = target.GetComponentsInChildren<T>();
            result.AddRange(components);
            return result;
        }

        /// <summary>
        /// Searches for a component in this <see cref="GameObject"/> or its children.
        /// </summary>
        /// <typeparam name="T">Type of the component/interface to search.</typeparam>
        /// <param name="target">The <see cref="GameObject"/> in which to search. </param>
        /// <param name="result"> The searched <typeparamref name="T"/> component or null otherwise. </param>
        /// <returns>True if the component/interface is found, False otherwise. </returns>
        public static bool TryGetComponentRecursive<T>(this GameObject target, [AllowNull] out T result) {
            result = target.GetComponentRecursive<T>();
            return result != null;
        }

        /// <inheritdoc cref="TryGetComponentRecursive{T}(GameObject, out T)"/>
        public static bool TryGetComponentInChildren<T>(this GameObject target, [AllowNull] out T result) => target.TryGetComponentRecursive<T>(out result);

        /// <summary>
        /// Searches for an interface of type <typeparamref name="T"/> in the <see cref="GameObject"/> components.
        /// </summary>
        /// <typeparam name="T">The type of the interface to search for. Must be an interface.</typeparam>
        /// <param name="target">The <see cref="GameObject"/> on which to search the interface <typeparamref name="T"/></param>
        /// <param name="Result">The interface <typeparamref name="T"/></param>
        /// <returns>True if the interface is found, false otherwise</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if the type <typeparamref name="T"/> is not an interface.</exception>
        public static bool TryGetInterface<T>(this GameObject target, [AllowNull] out T Result) where T : class {
            //Type check
            if (!typeof(T).IsInterface) {
                throw new System.InvalidOperationException($"Could not retrieve interface for type {typeof(T)}, as it is not an interface");
            }

            Result = target.GetComponent<T>();
            
            return Result != null;
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
        /// <returns>True if the object has all tags, false if one or more tags are missing or if the <see cref="GameObject"/> doesn't have the <see cref="Tags"/> component</returns>
        public static bool HasTags(this GameObject target, params string[] Tags) {
            if (!HasComponent<Tags>(target)) {
                Log.W($"Tried to look for tags {string.Concat(",", Tags)} in GameObject {target.name}, but no component of type {nameof(Tags)} has been found!");
                return false;
            }

            var tags = target.GetComponent<Tags>();
            return tags.Has(Tags);
        }
    }
}